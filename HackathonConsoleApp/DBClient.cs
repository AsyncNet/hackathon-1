using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackathonConsoleApp
{
    public class ConferenceInfo
    {
        public int ConfNo { get; set; } = 0;

        public string Title { get; set; } = default!;

    }

    internal class DBClient
    {
        private const string ConnectionString = "Server=WEUSHDXDB1.secure.dlgroup.com,5551\\DXSHARED;Database=dx_PIE_PIE1_main;User Id=BuildAdmin;Password=BuildAdmin;TrustServerCertificate=True";

        public IEnumerable<ConferenceInfo> Read(int confNo)
        {
            var conference = new List<ConferenceInfo>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand($"select [CONF_NO], [CONF_NAME] from [CONFERENCE] where [CONF_NO] = {confNo}", conn);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var movie = new ConferenceInfo()
                    {
                        ConfNo = Convert.ToInt32(reader["CONF_NO"]),
                        Title = reader["CONF_NAME"]?.ToString() ?? string.Empty,
                    };

                    conference.Add(movie);
                }
            }
            return conference;
        }
    }
}
