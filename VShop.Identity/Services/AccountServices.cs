using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;
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
        public readonly IEmailService _emailService;
        private readonly SignInManager<AppUser> _signInManager;
        protected AccountService(UserManager<AppUser> userManager, IEmailService emailService, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _emailService = emailService;
            _signInManager = signInManager;
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {
            LoginResponseDto response = new()
            {
                Id = "",
                Name = "",
                LastName = "",
                Cedula = "",
                Email = "",
                UserName = "",
                HasError = false,
                Errors = []
            };

            var user = await _userManager.FindByNameAsync(loginDto.UserName);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No existe una cuenta registrada con este nombre de usuario: {loginDto.UserName}");
                return response;
            }

            if (!user.EmailConfirmed || !user.Status)
            {
                response.HasError = true;
                response.Errors.Add($"Esta cuenta {loginDto.UserName} no está activa. Por favor verifica tu correo electrónico o espera aprobación del administrador.");
                return response;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, true);

            if (!result.Succeeded)
            {
                response.HasError = true;
                if (result.IsLockedOut)
                {
                    response.Errors.Add($"Tu cuenta {loginDto.UserName} ha sido bloqueada debido a múltiples intentos fallidos." +
                        $" Por favor intenta nuevamente en 10 minutos. Si no recuerdas tu contraseña, puedes realizar el proceso " +
                        $"de recuperación de contraseña.");
                }
                else
                {
                    response.Errors.Add($"Las credenciales son inválidas para este usuario: {user.UserName}");
                }
                return response;
            }

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
        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto, string? origin)
        {
            RegisterResponseDto response = new()
            {
                Id = "",
                Name = "",
                LastName = "",
                Cedula = "",
                Email = "",
                UserName = "",
                HasError = false,
                Errors = []
            };

            var userWithSameUserName = await _userManager.FindByNameAsync(saveDto.UserName);
            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add($"El nombre de usuario: {saveDto.UserName} ya está en uso.");
                return response;
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(saveDto.Email);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"El correo electrónico: {saveDto.Email} ya está registrado.");
                return response;
            }

            AppUser user = new AppUser()
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
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, saveDto.Role);

                string verificationUri = await GetVerificationEmailUri(user, origin);

                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = saveDto.Email,
                    HtmlBody = $@"
                    <!DOCTYPE html>
                    <html lang='es'>
                    <head>
                        <meta charset='UTF-8'>
                        <style>
                            body {{
                                font-family: 'Segoe UI', sans-serif;
                                background-color: #f8f9fa;
                                color: #2c3e50;
                                margin: 0;
                                padding: 0;
                            }}
                            .container {{
                                max-width: 620px;
                                margin: auto;
                                background-color: #ffffff;
                                border-radius: 8px;
                                box-shadow: 0 0 12px rgba(0, 0, 0, 0.05);
                                overflow: hidden;
                            }}
                            .header {{
                                background-color: #d35400;
                                padding: 20px;
                                text-align: center;
                                color: white;
                            }}
                            .header h1 {{
                                margin: 0;
                                font-size: 26px;
                            }}
                            .content {{
                                padding: 30px;
                            }}
                            .button {{
                                display: inline-block;
                                padding: 12px 24px;
                                background-color: #2c3e50;
                                color: #ffffff !important;
                                text-decoration: none;
                                border-radius: 5px;
                                margin-top: 20px;
                            }}
                            .footer {{
                                background-color: #f1f1f1;
                                text-align: center;
                                color: #888;
                                font-size: 12px;
                                padding: 15px;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>Bienvenido a Hermes Banking</h1>
                            </div>
                            <div class='content'>
                                <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>
                                <p>Gracias por registrarte en <strong>Hermes Banking</strong>. Tu cuenta ha sido creada exitosamente, pero necesitamos que confirmes tu correo electrónico para poder activarla.</p>
                                <p>Para completar tu registro y comenzar a utilizar nuestros servicios, haz clic en el siguiente botón:</p>
                                <a href='{verificationUri}' class='button'>Confirmar mi cuenta</a>
                                <p style='margin-top: 30px;'>Si no fuiste tú quien realizó este registro, por favor ignora este mensaje.</p>
                                <p>¡Gracias por confiar en Hermes Banking!</p>
                            </div>
                            <div class='footer'>
                                © 2025 Hermes Banking. Todos los derechos reservados.
                            </div>
                        </div>
                    </body>
                    </html>",
                    Subject = "Confirmación de registro"
                });
            }
            else
            {
                string? verificationUri = await GetVerificationEmailToken(user);
                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = saveDto.Email,
                    HtmlBody = $"Por favor confirma tu cuenta visitando este enlace {verificationUri}",
                    Subject = "Confirmación de registro"
                });
            }

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
            bool isNotcreated = !isCreated ?? false;
            EditResponseDto response = new()
            {
                Id = "",
                Name = "",
                LastName = "",
                Cedula = "",
                Email = "",
                UserName = "",
                HasError = false,
                Errors = []
            };


            var userWithSameUserName = await _userManager.Users.FirstOrDefaultAsync(w => w.UserName == saveDto.UserName && w.Id != saveDto.Id);
            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add($"El nombre de usuario: {saveDto.UserName} ya está en uso.");
                return response;
            }

            var userWithSameEmail = await _userManager.Users.FirstOrDefaultAsync(w => w.Email == saveDto.Email && w.Id != saveDto.Id);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"El correo electrónico: {saveDto.Email} ya está registrado.");
                return response;
            }

            var user = await _userManager.FindByIdAsync(saveDto.Id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No existe una cuenta registrada con este usuario");
                return response;
            }
            if (isNotcreated)
            {
                var rolesList = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, rolesList.ToList());
                await _userManager.AddToRoleAsync(user, saveDto.Role);
            }

            user.FirstName = saveDto.Name;
            user.LastName = saveDto.LastName;
            user.Cedula = saveDto.Cedula;
            user.UserName = saveDto.UserName;


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


            if (!string.IsNullOrWhiteSpace(saveDto.Password) && isNotcreated)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resultChange = await _userManager.ResetPasswordAsync(user, token, saveDto.Password);

                if (resultChange != null && !resultChange.Succeeded)
                {
                    response.HasError = true;
                    response.Errors.AddRange(resultChange.Errors.Select(s => s.Description).ToList());
                    return response;
                }
            }

            if (user.Status && !user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
            }


            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {

                if (!user.EmailConfirmed && isNotcreated)
                {
                    string verificationUri = await GetVerificationEmailUri(user, origin);
                    await _emailService.SendAsync(new EmailRequestDto()
                    {
                        To = saveDto.Email,
                        Subject = "Confirmación de correo",
                        HtmlBody = $@"
                        <!DOCTYPE html>
                        <html lang='es'>
                        <head>
                            <meta charset='UTF-8'>
                            <style>
                                body {{
                                    font-family: 'Segoe UI', sans-serif;
                                    background-color: #f8f9fa;
                                    color: #2c3e50;
                                    margin: 0;
                                    padding: 0;
                                }}
                                .container {{
                                    max-width: 620px;
                                    margin: auto;
                                    background-color: #ffffff;
                                    border-radius: 8px;
                                    box-shadow: 0 0 12px rgba(0, 0, 0, 0.05);
                                    overflow: hidden;
                                }}
                                .header {{
                                    background-color: #d35400;
                                    padding: 20px;
                                    text-align: center;
                                    color: white;
                                }}
                                .header h1 {{
                                    margin: 0;
                                    font-size: 24px;
                                }}
                                .content {{
                                    padding: 30px;
                                }}
                                .button {{
                                    display: inline-block;
                                    padding: 12px 24px;
                                    background-color: #2c3e50;
                                    color: #ffffff !important;
                                    text-decoration: none;
                                    border-radius: 5px;
                                    margin-top: 20px;
                                }}
                                .footer {{
                                    background-color: #f1f1f1;
                                    text-align: center;
                                    color: #888;
                                    font-size: 12px;
                                    padding: 15px;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h1>Verifica tu nuevo correo</h1>
                                </div>
                                <div class='content'>
                                    <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>
                                    <p>Hemos detectado que actualizaste tu dirección de correo electrónico en <strong>Hermes Banking</strong>.</p>
                                    <p>Para garantizar la seguridad de tu cuenta, es necesario que confirmes este nuevo correo.</p>
                                    <a href='{verificationUri}' class='button'>Confirmar correo electrónico</a>
                                    <p style='margin-top: 30px;'>Si no realizaste esta acción, por favor ignora este mensaje.</p>
                                    <p>Gracias por mantener tu información actualizada.</p>
                                </div>
                                <div class='footer'>
                                    © 2025 Hermes Banking. Todos los derechos reservados.
                                </div>
                            </div>
                        </body>
                        </html>"
                    });

                }

                var updatedRolesList = await _userManager.GetRolesAsync(user);

                response.Id = user.Id;
                response.Name = user.FirstName;
                response.LastName = user.LastName;
                response.Cedula = user.Cedula ?? "";
                response.Email = user.Email ?? "";
                response.UserName = user.UserName ?? "";
                response.IsVerified = user.EmailConfirmed;
                response.Roles = updatedRolesList.ToList();

                return response;
            }
            else
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }
        }

        public virtual async Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByNameAsync(request.UserName);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No existe una cuenta registrada con este nombre de usuario {request.UserName}");
                return response;
            }

            user.Status = false;
            await _userManager.UpdateAsync(user);

                string? resetToken = await GetResetPasswordToken(user);
                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = user.Email,
                    HtmlBody = $"Por favor restablece tu contraseña utilizando este token {resetToken}",
                    Subject = "Restablecer contraseña"
                });
            

            return response;
        }

        public virtual async Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(request.Id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No existe una cuenta registrada con este usuario");
                return response;
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }

            user.Status = true;
            user.EmailConfirmed = true;

            await _userManager.UpdateAsync(user);

            return response;
        }

        public virtual async Task<UserResponseDto> DeleteAsync(string id)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No existe una cuenta registrada con este usuario");
                return response;
            }

            await _userManager.DeleteAsync(user);

            return response;
        }

        public virtual async Task<UserDto?> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Name = user.FirstName,
                LastName = user.LastName,
                Cedula = user.Cedula ?? "",
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                isVerified = user.EmailConfirmed,
                Role = rolesList.FirstOrDefault() ?? "",
                Status = user.Status
            };

            return userDto;
        }
        public virtual async Task<UserDto?> GetUserById(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Name = user.FirstName,
                LastName = user.LastName,
                Cedula = user.Cedula ?? "",
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                isVerified = user.EmailConfirmed,
                Role = rolesList.FirstOrDefault() ?? "",
                Status = user.Status

            };

            return userDto;
        }
        public virtual async Task<UserDto?> GetUserByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Name = user.FirstName,
                LastName = user.LastName,
                Cedula = user.Cedula ?? "",
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                isVerified = user.EmailConfirmed,
                Role = rolesList.FirstOrDefault() ?? "",
                Status = user.Status

            };

            return userDto;
        }
        public virtual async Task<List<UserDto>> GetAllUser(bool? isActive = true)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (isActive == true)
            {
                usersQuery = usersQuery.Where(u => u.EmailConfirmed);
            }

            var users = await usersQuery.ToListAsync();

            var listUsersDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roleList = await _userManager.GetRolesAsync(user);

                listUsersDtos.Add(new UserDto()
                {
                    Id = user.Id,
                    Name = user.FirstName,
                    LastName = user.LastName,
                    Cedula = user.Cedula ?? "",
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    isVerified = user.EmailConfirmed,
                    Role = roleList.FirstOrDefault() ?? "",
                    Status = user.Status
                });
            }

            return listUsersDtos;
        }
        public virtual async Task<string> ConfirmAccountAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return "No existe una cuenta registrada con este usuario";
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                user.Status = true;
                await _userManager.UpdateAsync(user);

                return $"Cuenta confirmada para {user.Email}. Ya puedes usar la aplicación";
            }
            else
            {
                return $"Ocurrió un error al confirmar este correo electrónico {user.Email}";
            }
        }
        

        #region "Protected methods"

        protected async Task<string> GetVerificationEmailUri(AppUser user, string origin)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Login/ConfirmEmail";
            var completeUrl = new Uri(string.Concat(origin, "/", route));
            var verificationUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri.ToString(), "token", token);

            return verificationUri;
        }
        protected async Task<string> GetResetPasswordUri(AppUser user, string origin)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Login/ResetPassword";
            var completeUrl = new Uri(string.Concat(origin, "/", route));
            var resetUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            resetUri = QueryHelpers.AddQueryString(resetUri.ToString(), "token", token);

            return resetUri;
        }

        protected async Task<string?> GetVerificationEmailToken(AppUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }
        protected async Task<string?> GetResetPasswordToken(AppUser user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }
        #endregion
    }
}