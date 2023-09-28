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
        public string Description { get; set; } = default!;

    }

    internal class DBClient
    {
        const string sqltemplate = @"select
--'{ ""prompt"": ""' +
(CASE WHEN x.CONF_NAME IS NULL THEN '' ELSE 'Conference title is: ' + TRIM(x.CONF_NAME) + '.' END) + 
(CASE WHEN x.EVENT_DESCRIPTION IS NULL THEN '' ELSE 'Conference description is: ' + TRIM(x.EVENT_DESCRIPTION) + '.' END) + 
(CASE WHEN x.CP_DESCRIPTION IS NULL THEN '' ELSE 'Other description is: ' + TRIM(x.CP_DESCRIPTION) + '.' END)  +
(CASE WHEN x.CITY IS NULL THEN '' ELSE 'City: ' + TRIM(x.CITY) + '.' END) + 
(CASE WHEN x.LOCATIONS IS NULL THEN '' ELSE 'Conference location: ' + TRIM(x.LOCATIONS) + '.' END) + 
(CASE WHEN x.PRESENTER_NAMES IS NULL THEN '' ELSE 'Presenter names: ' + TRIM(x.PRESENTER_NAMES) + '.' END) + 
(CASE WHEN x.PRESENTER_SECTORS IS NULL THEN '' ELSE 'Presenters sectors: ' + TRIM(x.PRESENTER_SECTORS) + '.' END) + 
(CASE WHEN x.CONF_SECTORS IS NULL THEN '' ELSE 'Conference sectors: ' + TRIM(x.CONF_SECTORS) + '.' END) + 
--'"", ""completion"": ""' +
--'' + (CASE WHEN x.CONF_TARGETS IS NULL THEN '' ELSE '' + TRIM(x.CONF_TARGETS) END) +
--'"" }' as PROMPT
'' as Description
from (
	select
	REPLACE(c.CONF_NO, '""', ' ') CONF_NO
	,REPLACE(c.EVENT_DESCRIPTION, '""', ' ') EVENT_DESCRIPTION
	,REPLACE(c.CONF_NAME, '""', ' ') CONF_NAME
	,REPLACE(c.CITY, '""', ' ') CITY
	,REPLACE(CONVERT(VARCHAR(MAX), c.CP_DESCRIPTION), '""', ' ') CP_DESCRIPTION
	,(select STRING_AGG(CONVERT(NVARCHAR(max),TRIM(REPLACE(clx.CITY, '""', ' '))), ', ') from (select distinct cl.CITY from CONF_LOCATION cl where cl.CONF_NO = c.CONF_NO) clx) LOCATIONS
	,(select STRING_AGG(CONVERT(NVARCHAR(max),TRIM(REPLACE(cpx.PRESENTER_NAME, '""', ' '))), ', ') from (select distinct cp.PRESENTER_NAME from CONF_PRESENTER cp where cp.CONF_NO = c.CONF_NO) cpx) PRESENTER_NAMES
	,
	(
		select STRING_AGG(CONVERT(NVARCHAR(max),TRIM(REPLACE(sx.SECTOR_NAME, '""', ' '))), ', ') 
		from (
			select distinct s.SECTOR_NAME
			from CONF_PRESENTER cp
			left join CONF_PRESENTER_SECTOR eps on eps.CONF_PRESENTER_ID = cp.CONF_PRESENTER_ID and eps.CONF_NO = c.CONF_NO
			left join SECTOR s on s.SECTOR_SYMBOL = eps.SECTOR_SYMBOL
			where cp.CONF_NO = c.CONF_NO
		) 
		sx
	) PRESENTER_SECTORS
	,
	(
		select STRING_AGG(CONVERT(NVARCHAR(max),TRIM(REPLACE(s2x.SECTOR_NAME, '""', ' '))), ', ') 
		from (
			select distinct s2.SECTOR_NAME
			from CONF_SECTOR cs2
			left join SECTOR s2 on s2.SECTOR_SYMBOL = cs2.SECTOR_SYMBOL
			where cs2.CONF_NO = c.CONF_NO
		) 
		s2x
	) CONF_SECTORS
	,
	(
		select STRING_AGG(
			CONVERT(
				NVARCHAR(max), 
				--'{ \""id\"": \""' + ctx.PORTFOLIO_NO + '\"", \""email\"": \""' + REPLACE(ctx.EMAIL_ADDRESS, '""',' ') + '\"" }'
				REPLACE(ctx.EMAIL_ADDRESS, '""','\""')
				), 
			', '
			) 
		from (
			select distinct CONVERT(VARCHAR(MAX),ct.PORTFOLIO_NO) PORTFOLIO_NO, TRIM(ct.EMAIL_ADDRESS) EMAIL_ADDRESS
			from CONF_TARGET ct
			left join PORTFOLIO p on p.PORTFOLIO_NO = ct.PORTFOLIO_NO
			where ct.CONF_NO = c.CONF_NO
			and ct.REGISTRATION_TYPE = 'I'
			and ct.REG_STATUS = 'R'
			and ct.PORTFOLIO_NO is not null
		) 
		ctx
	) CONF_TARGETS
	from CONFERENCE c
	where c.CONF_NO = @confno
) x
";

        private const string ConnectionString = "Server=WEUSHDXDB1.secure.dlgroup.com,5551\\DXSHARED;Database=dx_PIE_PIE1_main;User Id=BuildAdmin;Password=BuildAdmin;TrustServerCertificate=True";

        public IEnumerable<ConferenceInfo> Read(int confNo)
        {
            var conference = new List<ConferenceInfo>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(sqltemplate, conn);

				cmd.Parameters.Add(new SqlParameter("@confno", 875));

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var movie = new ConferenceInfo()
                    {
                        Description = reader["Description"]?.ToString() ?? string.Empty,
                    };

                    conference.Add(movie);
                }
            }
            return conference;
        }
    }
}
