using AppLogin.Data;
using AppLogin.Models;
using AppLogin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AppLogin.Controllers
{
    public class AccesoController : Controller
    {
        private readonly AppDBContext _appDBContext;
        public AccesoController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }
        [HttpGet]
        public IActionResult Registrarse()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrarse(UsuarioVM usuarioVM)
        {
            if(usuarioVM.Clave != usuarioVM.ConfirmarClave)
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden";
                return View();
            }

            Usuario usuario = new Usuario
            {
                NombreCompleto = usuarioVM.NombreCompleto,
                Correo = usuarioVM.Correo,
                Clave = usuarioVM.Clave
            };

            await _appDBContext.Usuarios.AddAsync(usuario);
            await _appDBContext.SaveChangesAsync();

            if(usuario.IdUsuario != 0) return RedirectToAction("Login", "Acceso");
            //caso contrario
            ViewData["Mensaje"] = "Las contraseñas no coinciden";
            return View(usuarioVM);
        }

        [HttpGet]
        public IActionResult Login()
        {   if(User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            Usuario? usuario_emcontrado = await _appDBContext.Usuarios
                .Where(u => 
                    u.Correo == loginVM.Correo && 
                    u.Clave == loginVM.Clave
                    ).FirstOrDefaultAsync();

            if (usuario_emcontrado == null)
            {
                ViewData["Mensaje"] = "Correo o contraseña incorrectos";
                return View();
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario_emcontrado.NombreCompleto)
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
            );

            return RedirectToAction("Index", "Home");
        }

    }
}
