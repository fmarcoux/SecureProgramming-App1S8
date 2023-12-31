using CoupDeSonde.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

using System.Reflection;
using System.Text.RegularExpressions;
using static CoupDeSonde.Controllers.SurveyController;

namespace CoupDeSonde.Controllers
{
    [ApiController]
    [Route("[controller]")]
     
    public class SurveyController : ControllerBase
    {
        Authentification _authenticator = new Authentication.Authenticator();
        
        public class Survey
        {
            //there are two possible surveys: 1 and 2
            public int SurveyNumber { get; set; }
            //the answers are concatenated under a string form: 'abcd'
            public string Answers { get; set; }
            //the questions of the survey depend on the SurveyNumber
            public string Question1 { get; set; }
            public string Question2 { get; set; }
            public string Question3 { get; set; }
            public string Question4 { get; set; }

        }


        public static void CreateSurvey(Survey inputSurvey, int surveyNumber)
        {

            inputSurvey.Question1 = "1. À quelle tranche d'âge appartenez-vous? a:0-25 ans, b:25-50 ans, c:50-75 ans, d:75 ans et plus\n\n";
            inputSurvey.Question2 = "2. Êtes-vous une femme ou un homme? a:Femme, b:Homme, c:Je ne veux pas répondre\n\n";
            inputSurvey.Answers = "";
            inputSurvey.SurveyNumber = surveyNumber;

            if (surveyNumber == 1)
            {   
                inputSurvey.Question3 = "3. Quel journal lisez-vous à la maison? a:La Presse, b:Le Journal de Montréal, c:The Gazette, d:Le Devoir\n\n";
                inputSurvey.Question4 = "4. Combien de temps accordez-vous à la lecture de votre journal quotidiennement? a:Moins de 10 minutes; b:Entre 10 et 30 minutes, c:Entre 30 et 60 minutes, d:60 minutes ou plus\n\n";
            }
            else
            {
                inputSurvey.Question3 = "3. Combien de tasses de café buvez-vous chaque jour? a: Je ne bois pas de café, b:Entre 1 et 5 tasses, c: Entre 6 et 10 tasses, d: 10 tasses ou plus\n\n";
                inputSurvey.Question4 = "4. Combien de consommations alcoolisées buvez-vous chaque jour? a: 0, b: 1, c: 2 ou 3, d: 3 ou plus\n\n";
            }
        }

        [HttpGet("sondage")]
        public ActionResult<Survey> GetSurvey()
        { 
            if (HttpContext.Request.Headers.ContainsKey("X-API-KEY"))
            {
                string apiKey = HttpContext.Request.Headers["X-API-KEY"];

                //Input validation to prevent injection attacks
                if (!apiKeyInputValidator(apiKey))
                {
                    return BadRequest();
                }
                //should check whether the key is good or not
                string username = _authenticator.Authenticate(apiKey);
                Console.WriteLine(username);

                if (username != "ERREUR")
                {
                    //select and create randomly one of the two surveys
                    Random rand = new Random();
                    int surveyNumber = rand.Next(1, 3);
                    Console.WriteLine($"Submitting survey number {surveyNumber}");

                    Survey requested = new Survey();
                    CreateSurvey(requested, surveyNumber);
                    return Ok(requested);
                }
                else
                {
                    return Unauthorized();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("sondageAll")]
        public ActionResult<List<Survey>> GetAllSurvey()
        {
            if (HttpContext.Request.Headers.ContainsKey("X-API-KEY"))
            {
                string apiKey = HttpContext.Request.Headers["X-API-KEY"];

                //Input validation to prevent injection attacks
                if (!apiKeyInputValidator(apiKey))
                {
                    return BadRequest();
                }

                //should check whether the key is good or not
                string username = _authenticator.Authenticate(apiKey);
                Console.WriteLine(username);

                if (username != "ERREUR" )
                {
                    //select and create randomly one of the two surveys
                    Random rand = new Random();
                    int surveyNumber = rand.Next(1, 3);
                    Console.WriteLine($"Submitting survey number {surveyNumber}");

                    string dbPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "surveyResults.db");

                    string connString = string.Format("Data Source={0}", dbPath);
                    var connection = new SqliteConnection(connString);
                    connection.Open();
                    Console.WriteLine("Connection to DB established");

                    SqliteCommand sqlite_cmd;
                    sqlite_cmd = connection.CreateCommand();
                    sqlite_cmd.CommandText = String.Format($"Select * from resultats");
                    var sqlite_datareader = sqlite_cmd.ExecuteReader();

                    var surveyList = new List<Survey>();
                    while (sqlite_datareader.Read())
                    {
                        string surveyNb = sqlite_datareader.GetString(0);
                        string answer = sqlite_datareader.GetString(1);
                        Survey survey = new Survey();
                        CreateSurvey(survey, Int32.Parse(surveyNb));
                        survey.Answers = answer;
                        surveyList.Add(survey);
                    }
                    sqlite_datareader.Close();                    

                    return Ok(surveyList);

                }
                else
                {
                    return Unauthorized();
                }

            }
            else
            {
                return BadRequest();
            }
        }


        [HttpPost("sondage")]
        public ActionResult<string> PostSurvey(Survey survey)
        {
            if (HttpContext.Request.Headers.ContainsKey("X-API-KEY"))
            {
                string apiKey = HttpContext.Request.Headers["X-API-KEY"];

                //Input validation to prevent injection attacks
                if (!apiKeyInputValidator(apiKey))
                {
                    return BadRequest();
                }

                //should check whether the key is good or not
                string username = _authenticator.Authenticate(apiKey);
                Console.WriteLine(username);

                if(!surveyInputValidator(survey.SurveyNumber, survey.Answers))
                {
                    return BadRequest();
                };

                if (username != "ERREUR")
                {
                    Console.WriteLine("Request received");

                    string dbPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "surveyResults.db");

                    string connString = string.Format("Data Source={0}", dbPath);
                    var connection = new SqliteConnection(connString);
                    connection.Open();
                    Console.WriteLine("Connection to DB established");

                    SqliteCommand sqlite_cmd;
                    sqlite_cmd = connection.CreateCommand();
                    sqlite_cmd.CommandText = String.Format($"INSERT INTO resultats(SurveyID,Answer)VALUES({survey.SurveyNumber},'{survey.Answers}')");

                    var sqlite_datareader = sqlite_cmd.ExecuteReader();
                    sqlite_datareader.Close();

                    return Ok();         
                   
                }
                else
                {
                    return Unauthorized();
                }

            }
            else
            {
                return BadRequest();
            }
        }
        private bool apiKeyInputValidator(String apiKey)

        {
            // {83884C08-A054-4CA8-A3D5-4C2C23F48E70}

            Regex validAPI = new Regex(@"^[a-zA-Z0-9]*[\-][a-zA-Z0-9]*[\-][a-zA-Z0-9]*[\-][a-zA-Z0-9]*[\-][a-zA-Z0-9]*$");

            if (validAPI.IsMatch(apiKey) & apiKey.Length == 36)

            {

                return true;

            }
            else
            {
                return false;
            }
        }
        private bool surveyInputValidator(int surveyNumber, String answer)
        {
            Regex validSurveyNumber = new Regex("^[0-9]*$");

            Regex validAnswer = new Regex("^[a-zA-Z]*$");

            if (validSurveyNumber.IsMatch(surveyNumber.ToString()) & validAnswer.IsMatch(answer) & answer.Length == 4)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void SetAuthenticator(Authentification authenticator)
        {
            _authenticator = authenticator;
        }


    }
}