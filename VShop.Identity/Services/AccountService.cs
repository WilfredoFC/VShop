using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using VShop.Application.Dtos.Email;
using VShop.Application.Dtos.Login;
using VShop.Application.Dtos.User;
using VShop.Application.Interfaces;
using VShop.Identity.Entities;

namespace VShop.Identity.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;

        public AccountService(UserManager<AppUser> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {
            LoginResponseDto response = new()
            {
                Id = "", Name = "", LastName = "", Cedula = "",
                Email = "", UserName = "", HasError = false, Errors = []
            };

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No existe una cuenta registrada con el correo: {loginDto.Email}");
                return response;
            }

            if (!user.EmailConfirmed || !user.Status)
            {
                response.HasError = true;
                response.Errors.Add($"La cuenta {loginDto.Email} no está activa. Verifica tu correo o espera aprobación del administrador.");
                return response;
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                response.HasError = true;
                response.Errors.Add($"Tu cuenta ha sido bloqueada temporalmente por múltiples intentos fallidos. Intenta en unos minutos.");
                return response;
            }

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                await _userManager.AccessFailedAsync(user);
                if (await _userManager.IsLockedOutAsync(user))
                    response.Errors.Add($"Tu cuenta {loginDto.Email} ha sido bloqueada. Intenta en 1 minuto o restablece tu contraseña.");
                else
                    response.Errors.Add("Las credenciales son inválidas.");
                response.HasError = true;
                return response;
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            var rolesList = await _userManager.GetRolesAsync(user);
            response.Id = user.Id;
            response.Name = user.FirstName;
            response.LastName = user.LastName;
            response.Cedula = user.Cedula ?? "";
            response.Email = user.Email ?? "";
            response.UserName = user.UserName ?? "";
            response.IsVerified = user.EmailConfirmed;
            response.Roles = rolesList.ToList();
            return response;
        }

        // Sign-out is now handled at the presentation layer (controller/page).
        public Task SignOutAsync() => Task.CompletedTask;

        public async Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto, string? origin)
        {
            RegisterResponseDto response = new()
            {
                Id = "", Name = "", LastName = "", Cedula = "",
                Email = "", UserName = "", HasError = false, Errors = []
            };

            var userWithSameEmail = await _userManager.FindByEmailAsync(saveDto.Email);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"El correo electrónico {saveDto.Email} ya está registrado.");
                return response;
            }

            var user = new AppUser
            {
                FirstName = saveDto.Name,
                LastName = saveDto.LastName,
                Cedula = saveDto.Cedula,
                Email = saveDto.Email,
                UserName = saveDto.UserName,
                EmailConfirmed = false,
                Status = false
            };

            var result = await _userManager.CreateAsync(user, saveDto.Password);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(e => e.Description));
                return response;
            }

            await _userManager.AddToRoleAsync(user, saveDto.Role);

            string verificationUri = await GetVerificationEmailUri(user, origin ?? "");
            await _emailService.SendAsync(new EmailRequestDto
            {
                To = saveDto.Email,
                Subject = "Confirmación de registro",
                HtmlBody = BuildConfirmationEmail(user.FirstName, user.LastName, verificationUri)
            });

            var rolesList = await _userManager.GetRolesAsync(user);
            response.Id = user.Id;
            response.Name = user.FirstName;
            response.LastName = user.LastName;
            response.Cedula = user.Cedula ?? "";
            response.Email = user.Email ?? "";
            response.UserName = user.UserName ?? "";
            response.IsVerified = user.EmailConfirmed;
            response.Roles = rolesList.ToList();
            return response;
        }

        public async Task<EditResponseDto> EditUser(SaveUserDto saveDto, string origin, bool? isCreated = false)
        {
            bool isNotCreated = !isCreated ?? false;
            EditResponseDto response = new()
            {
                Id = "", Name = "", LastName = "", Cedula = "",
                Email = "", UserName = "", HasError = false, Errors = []
            };

            var userWithSameEmail = await _userManager.Users
                .FirstOrDefaultAsync(w => w.Email == saveDto.Email && w.Id != saveDto.Id);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"El correo electrónico {saveDto.Email} ya está registrado.");
                return response;
            }

            var user = await _userManager.FindByIdAsync(saveDto.Id);
            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add("No existe una cuenta registrada con este usuario.");
                return response;
            }

            if (isNotCreated)
            {
                var rolesList = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, rolesList.ToList());
                await _userManager.AddToRoleAsync(user, saveDto.Role);
            }

            user.FirstName = saveDto.Name;
            user.LastName = saveDto.LastName;
            user.Cedula = saveDto.Cedula;

            bool emailChanged = !string.Equals(user.Email, saveDto.Email, StringComparison.OrdinalIgnoreCase);
            if (emailChanged)
            {
                user.Email = saveDto.Email;
                user.EmailConfirmed = false;
                user.Status = false;
            }
            else
            {
                user.Email = saveDto.Email;
                user.Status = saveDto.Status;
                user.EmailConfirmed = user.EmailConfirmed && user.Email == saveDto.Email;
            }

            if (!string.IsNullOrWhiteSpace(saveDto.Password) && isNotCreated)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resultChange = await _userManager.ResetPasswordAsync(user, token, saveDto.Password);
                if (!resultChange.Succeeded)
                {
                    response.HasError = true;
                    response.Errors.AddRange(resultChange.Errors.Select(e => e.Description));
                    return response;
                }
            }

            if (user.Status && !user.EmailConfirmed)
                user.EmailConfirmed = true;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(e => e.Description));
                return response;
            }

            if (!user.EmailConfirmed && isNotCreated)
            {
                string verificationUri = await GetVerificationEmailUri(user, origin);
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = saveDto.Email,
                    Subject = "Confirmación de correo",
                    HtmlBody = BuildEmailChangedEmail(user.FirstName, user.LastName, verificationUri)
                });
            }

            var updatedRoles = await _userManager.GetRolesAsync(user);
            response.Id = user.Id;
            response.Name = user.FirstName;
            response.LastName = user.LastName;
            response.Cedula = user.Cedula ?? "";
            response.Email = user.Email ?? "";
            response.UserName = user.UserName ?? "";
            response.IsVerified = user.EmailConfirmed;
            response.Roles = updatedRoles.ToList();
            return response;
        }

        public async Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
                return response; // No revelar si el usuario existe o no

            string resetUri = await GetResetPasswordUri(user, request.Origin ?? "");
            await _emailService.SendAsync(new EmailRequestDto
            {
                To = user.Email,
                Subject = "Restablecer contraseña",
                HtmlBody = BuildResetPasswordEmail(user.FirstName, user.LastName, resetUri)
            });

            return response;
        }

        public async Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add("No existe una cuenta registrada con este usuario.");
                return response;
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(e => e.Description));
                return response;
            }

            user.Status = true;
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            return response;
        }

        public async Task<UserResponseDto> DeleteAsync(string id)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add("No existe una cuenta registrada con este usuario.");
                return response;
            }
            await _userManager.DeleteAsync(user);
            return response;
        }

        public async Task<UserDto?> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;
            var roles = await _userManager.GetRolesAsync(user);
            return ToDto(user, roles);
        }

        public async Task<UserDto?> GetUserById(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null) return null;
            var roles = await _userManager.GetRolesAsync(user);
            return ToDto(user, roles);
        }

        public async Task<UserDto?> GetUserByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return null;
            var roles = await _userManager.GetRolesAsync(user);
            return ToDto(user, roles);
        }

        public async Task<List<UserDto>> GetAllUser(bool? isActive = true)
        {
            var query = _userManager.Users.AsQueryable();
            if (isActive == true)
                query = query.Where(u => u.EmailConfirmed);

            var users = await query.ToListAsync();
            var result = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(ToDto(user, roles));
            }
            return result;
        }

        public async Task<string> ConfirmAccountAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return "No existe una cuenta registrada con este usuario.";

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return $"Ocurrió un error al confirmar el correo {user.Email}.";

            user.Status = true;
            await _userManager.UpdateAsync(user);
            return $"Cuenta confirmada para {user.Email}. Ya puedes usar la aplicación.";
        }

        #region Private helpers

        private static UserDto ToDto(AppUser user, IList<string> roles) => new()
        {
            Id = user.Id,
            Name = user.FirstName,
            LastName = user.LastName,
            Cedula = user.Cedula ?? "",
            Email = user.Email ?? "",
            UserName = user.UserName ?? "",
            isVerified = user.EmailConfirmed,
            Role = roles.FirstOrDefault() ?? "",
            Status = user.Status
        };

        private async Task<string> GetVerificationEmailUri(AppUser user, string origin)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = new Uri(string.Concat(origin.TrimEnd('/'), "/Auth/ConfirmEmail"));
            var uri = QueryHelpers.AddQueryString(url.ToString(), "userId", user.Id);
            return QueryHelpers.AddQueryString(uri, "token", token);
        }

        private async Task<string> GetResetPasswordUri(AppUser user, string origin)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = new Uri(string.Concat(origin.TrimEnd('/'), "/Auth/ResetPassword"));
            var uri = QueryHelpers.AddQueryString(url.ToString(), "userId", user.Id);
            return QueryHelpers.AddQueryString(uri, "token", token);
        }

        private static string BuildConfirmationEmail(string firstName, string lastName, string verificationUri) => $@"
<!DOCTYPE html><html lang='es'><head><meta charset='UTF-8'>
<style>
  body{{font-family:'Segoe UI',sans-serif;background:#f8f9fa;color:#2c3e50;margin:0;padding:0}}
  .wrap{{max-width:620px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 0 12px rgba(0,0,0,.05);overflow:hidden}}
  .hdr{{background:linear-gradient(135deg,#667eea,#764ba2);padding:24px;text-align:center;color:#fff}}
  .hdr h1{{margin:0;font-size:26px}}
  .body{{padding:32px}}
  .btn{{display:inline-block;padding:12px 28px;background:#667eea;color:#fff!important;text-decoration:none;border-radius:6px;margin-top:20px;font-weight:600}}
  .foot{{background:#f1f1f1;text-align:center;color:#888;font-size:12px;padding:16px}}
</style></head>
<body><div class='wrap'>
  <div class='hdr'><h1>Bienvenido a VShop</h1></div>
  <div class='body'>
    <p>Hola <strong>{firstName} {lastName}</strong>,</p>
    <p>Gracias por registrarte en <strong>VShop</strong>. Para activar tu cuenta confirma tu correo electrónico:</p>
    <a href='{verificationUri}' class='btn'>Confirmar mi cuenta</a>
    <p style='margin-top:28px;color:#888;font-size:13px'>Si no realizaste este registro, ignora este mensaje.</p>
  </div>
  <div class='foot'>© {DateTime.UtcNow.Year} VShop. Todos los derechos reservados.</div>
</div></body></html>";

        private static string BuildEmailChangedEmail(string firstName, string lastName, string verificationUri) => $@"
<!DOCTYPE html><html lang='es'><head><meta charset='UTF-8'>
<style>
  body{{font-family:'Segoe UI',sans-serif;background:#f8f9fa;color:#2c3e50;margin:0;padding:0}}
  .wrap{{max-width:620px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 0 12px rgba(0,0,0,.05);overflow:hidden}}
  .hdr{{background:linear-gradient(135deg,#667eea,#764ba2);padding:24px;text-align:center;color:#fff}}
  .hdr h1{{margin:0;font-size:24px}}
  .body{{padding:32px}}
  .btn{{display:inline-block;padding:12px 28px;background:#667eea;color:#fff!important;text-decoration:none;border-radius:6px;margin-top:20px;font-weight:600}}
  .foot{{background:#f1f1f1;text-align:center;color:#888;font-size:12px;padding:16px}}
</style></head>
<body><div class='wrap'>
  <div class='hdr'><h1>Verifica tu nuevo correo</h1></div>
  <div class='body'>
    <p>Hola <strong>{firstName} {lastName}</strong>,</p>
    <p>Detectamos que actualizaste tu dirección de correo en <strong>VShop</strong>. Confirma el nuevo correo para mantener el acceso a tu cuenta:</p>
    <a href='{verificationUri}' class='btn'>Confirmar correo electrónico</a>
    <p style='margin-top:28px;color:#888;font-size:13px'>Si no realizaste esta acción, ignora este mensaje.</p>
  </div>
  <div class='foot'>© {DateTime.UtcNow.Year} VShop. Todos los derechos reservados.</div>
</div></body></html>";

        private static string BuildResetPasswordEmail(string firstName, string lastName, string resetUri) => $@"
<!DOCTYPE html><html lang='es'><head><meta charset='UTF-8'>
<style>
  body{{font-family:'Segoe UI',sans-serif;background:#f8f9fa;color:#2c3e50;margin:0;padding:0}}
  .wrap{{max-width:620px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 0 12px rgba(0,0,0,.05);overflow:hidden}}
  .hdr{{background:linear-gradient(135deg,#e74c3c,#c0392b);padding:24px;text-align:center;color:#fff}}
  .hdr h1{{margin:0;font-size:24px}}
  .body{{padding:32px}}
  .btn{{display:inline-block;padding:12px 28px;background:#e74c3c;color:#fff!important;text-decoration:none;border-radius:6px;margin-top:20px;font-weight:600}}
  .foot{{background:#f1f1f1;text-align:center;color:#888;font-size:12px;padding:16px}}
</style></head>
<body><div class='wrap'>
  <div class='hdr'><h1>Restablecer contraseña</h1></div>
  <div class='body'>
    <p>Hola <strong>{firstName} {lastName}</strong>,</p>
    <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta en <strong>VShop</strong>. Haz clic en el botón para continuar:</p>
    <a href='{resetUri}' class='btn'>Restablecer contraseña</a>
    <p style='margin-top:28px;color:#888;font-size:13px'>Este enlace expira en 12 horas. Si no solicitaste este cambio, ignora este mensaje — tu contraseña no será modificada.</p>
  </div>
  <div class='foot'>© {DateTime.UtcNow.Year} VShop. Todos los derechos reservados.</div>
</div></body></html>";

        #endregion
    }
}
