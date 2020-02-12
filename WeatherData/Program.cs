//BASE
using System;
using System.Collections.Generic;//For lists
using System.Threading;
//DATA
using System.Data;
using System.Data.SqlClient;
//JSON
using Newtonsoft.Json;
//API
using RestSharp;
using RestSharp.Authenticators;


namespace WeatherData
{
    class Program
    {
        public static void Main(string[] args)
        {
            var zip = FindZip();
            API_Data(zip);
        }
        public static List<string> FindZip()
        {
            using (SqlConnection connection = new SqlConnection("Server=localhost; Database=AgroPro; User Id=app; Password=Password123;"))
            {
                List<string> zips = new List<string>();

                var SP = "[dbo].[getZipcodes]";

                SqlCommand command = new SqlCommand(SP, connection);

                connection.Open();

                command.CommandType = CommandType.StoredProcedure;

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    zips.Add(reader["zipcode"].ToString());
                }

                reader.Close();

                return zips;
            }
        }
        public static void API_Data(List<string> zipcodes)
        {
            for(int x = 0; x < zipcodes.Count; x++)
            {
                var client = new RestClient("https://api.openweathermap.org/data/2.5");

                var request = new RestRequest("weather", Method.GET);
                request.AddParameter("zip", zipcodes[x]);
                request.AddParameter("units", "imperial");
                request.AddParameter("APPID", "450d273db4b600c520071597193e0ce1");

                IRestResponse response = client.Execute(request);
                var json = response.Content;

                Rootobject weather = JsonConvert.DeserializeObject<Rootobject>(json);

                using (SqlConnection connection = new SqlConnection("Server=localhost; Database=DataCollection; User Id=app; Password=Password123;"))
                {
                    var SP = "[dbo].[StoreWeatherData]";

                    SqlCommand command = new SqlCommand(SP, connection);

                    connection.Open();

                    command.CommandType = CommandType.StoredProcedure;
                    //coord
                    command.Parameters.AddWithValue("@lon", weather.coord.lon);
                    command.Parameters.AddWithValue("@lat", weather.coord.lat);
                    //weather
                    command.Parameters.AddWithValue("@wid", weather.weather[0].id);
                    command.Parameters.AddWithValue("@main", weather.weather[0].main);
                    command.Parameters.AddWithValue("@description", weather.weather[0].description);
                    command.Parameters.AddWithValue("@icon", weather.weather[0].id);
                    //base
                    command.Parameters.AddWithValue("@base", weather._base);
                    //main
                    command.Parameters.AddWithValue("@temp", weather.main.temp);
                    command.Parameters.AddWithValue("@feels_like", weather.main.feels_like);
                    command.Parameters.AddWithValue("@temp_min", weather.main.temp_min);
                    command.Parameters.AddWithValue("@temp_max", weather.main.temp_max);
                    command.Parameters.AddWithValue("@pressure", weather.main.pressure);
                    command.Parameters.AddWithValue("@humidity", weather.main.humidity);
                    //visibility
                    command.Parameters.AddWithValue("@visibility", weather.main.humidity);
                    //Wind
                    command.Parameters.AddWithValue("@speed", weather.wind.speed);
                    command.Parameters.AddWithValue("@deg", weather.wind.deg);
                    //cloud
                    command.Parameters.AddWithValue("@all", weather.clouds.all);
                    //dt
                    command.Parameters.AddWithValue("@dt", weather.dt);
                    //sys
                    command.Parameters.AddWithValue("@type", weather.sys.type);
                    command.Parameters.AddWithValue("@sysid", weather.sys.id);
                    command.Parameters.AddWithValue("@country", weather.sys.country);
                    command.Parameters.AddWithValue("@sunrise", weather.sys.sunrise);
                    command.Parameters.AddWithValue("@sunset", weather.sys.sunset);
                    //end
                    command.Parameters.AddWithValue("@timezone", weather.timezone);
                    command.Parameters.AddWithValue("@id", weather.id);
                    command.Parameters.AddWithValue("@name", weather.name);
                    command.Parameters.AddWithValue("@cod", weather.cod);

                    command.ExecuteReader();
                }
            }
        }
    }
    public class Rootobject
    {
        public Coord coord { get; set; }
        public Weather[] weather { get; set; }
        public string _base { get; set; }
        public Main main { get; set; }
        public int visibility { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int timezone { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }

    public class Coord
    {
        public float lon { get; set; }
        public float lat { get; set; }
    }

    public class Main
    {
        public float temp { get; set; }
        public float feels_like { get; set; }
        public float temp_min { get; set; }
        public float temp_max { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
    }

    public class Wind
    {
        public float speed { get; set; }
        public int deg { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Sys
    {
        public int type { get; set; }
        public int id { get; set; }
        public string country { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

}
