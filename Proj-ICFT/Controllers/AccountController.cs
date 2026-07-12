using Firebase.Auth;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Proj_ICFT.Models;
using Proj_ICFT.Models.DTO;
using Proj_ICFT.Models.Filter;
using Proj_ICFT.Models.ModelUrl;
using System.Text;

namespace Proj_ICFT.Controllers
{
    public class AccountController : Controller
    {
        FirebaseAuthProvider auth;
        private readonly FeatureFlags _features;

        public AccountController(IOptions<FeatureFlags> features)
        {
            auth = new FirebaseAuthProvider(
                            new FirebaseConfig("AIzaSyCY6ZiTuU3iDVMe37SK2p1oWCCoe7ltEV4"));
            _features = features.Value;
        }

        private bool Logado() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString("_UserToken"));
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            if (!_features.CadastroPublico && !Logado())
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signin2(AdministradorDTO Adm)
        {

            try
            {
                ModelUrlJson url = new ModelUrlJson();

                FirebaseAuthLink firebaseAuthLink = null;


                //log in the user
                firebaseAuthLink = await auth
                                .SignInWithEmailAndPasswordAsync(Adm.Email, Adm.Senha);
                if (firebaseAuthLink.FirebaseToken != null)
                {
                    HttpContext.Session.SetString("_UserToken", firebaseAuthLink.FirebaseToken);
                    HttpContext.Session.SetString("_UserEmail", Adm.Email);
                    var usertoken = HttpContext.Session.GetString("_UserToken");
                    HttpClient httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {firebaseAuthLink.FirebaseToken}");



                    //TokenJson token = JsonConvert.DeserializeObject<TokenJson>(usertoken);




                    if (FirebaseApp.DefaultInstance == null)
                        FirebaseApp.Create(new AppOptions()
                        {
                            //caminho local 
                            Credential = GoogleCredential.FromFile(url.UrlJson),
                        });


                    var decodedToken = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(usertoken);

                    var Refreshtoken = VerifyAndRefreshToken(usertoken);
                    HttpContext.Session.SetString("_UserToken", Refreshtoken);
                    object isAdmin;
                    //if (decodedToken.Claims.TryGetValue("admin", out isAdmin))

                    //{
                    //    if ((bool)isAdmin)
                    //    {
                    //        return View("Administrador");

                    //    }
                    //    else
                    //    {
                    //        return View("Usuario");
                    //    }
                    //}
                    //else
                    //{
                        return RedirectToAction("Form", "Home");
                        // return View("Login");
                    //}
                }
                else
                {
                    TempData["ErrorMessage"] = "Não foi possivel realizar o login. Verifique seus dados e tente novamente em alguns instantes";

                    return View("Login");
                }
            }

            catch (Firebase.Auth.FirebaseAuthException)
            {
                TempData["ErrorMessage"] = "E-mail ou senha incorretos.";
                return View("Login");
            }
            catch
            {
                TempData["ErrorMessage"] = "Não foi possível realizar o login. Tente novamente em alguns instantes.";
                return View("Login");
            }
        }

        static string VerifyAndRefreshToken(string idToken)
        {
            try
            {
                var decodedToken = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken).Result;

                // Verifique se o token está próximo de expirar (por exemplo, em menos de 5 minutos)
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(decodedToken.ExpirationTimeSeconds).UtcDateTime;
                var currentTime = DateTime.UtcNow;
                var timeUntilExpiration = expirationTime - currentTime;
                var refreshThreshold = TimeSpan.FromMinutes(5);

                if (timeUntilExpiration < refreshThreshold)
                {
                    // O token está próximo de expirar, faça o refresh
                    var newToken = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.CreateSessionCookieAsync(idToken, new SessionCookieOptions()
                    {
                        ExpiresIn = TimeSpan.FromDays(1), // Expiração de 24 horas
                    }).Result;

                    return newToken;
                }
                else
                {
                    // O token ainda é válido
                    return idToken;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Token inválido ou expirado", ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> NewRegister(AdministradorDTO user)
        {
            if (!_features.CadastroPublico && !Logado())
                return RedirectToAction("Login");

            try
            {
                ModelUrlJson url = new ModelUrlJson();


                await auth.CreateUserWithEmailAndPasswordAsync(user.Email, user.Senha);
                var newUser = await auth.SignInWithEmailAndPasswordAsync(user.Email, user.Senha);

                // criação de role ADMIN para novos usuarios cadastrados pelo administrador
                var token = newUser.FirebaseToken;

                var claims = new Dictionary<string, object>()
                {
                    { "admin", true },
                };
                if (FirebaseApp.DefaultInstance == null)

                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(url.UrlJson),
                    });
                }
                var decoded = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
                var uid = decoded.Uid;
                await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(uid, claims);

                TempData["SuccessMessage"] = "Usuario Cadastrado com Sucesso";
                return View("Register");
            }
            catch (Firebase.Auth.FirebaseAuthException ex)
            {
                if (ex.Reason.ToString() == "EmailExists")
                {
                    TempData["ErrorMessage"] = "Email já pertence a um administrador";
                    return View("Register");
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocorrreu um erro!";
                    return View("Register");
                }
            }
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [SessionFilter]
        public async Task<IActionResult> Logout()
        {
            string token = "";

            HttpContext.Session.SetString("_UserToken", token);
            return View("Login");

        }

    }
}
        

    

