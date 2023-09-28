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

		const string insertsql = @"INSERT INTO [dbo].[CONF_TARGET]
           ([CONF_NO]
           ,[CONF_TARGET_NO]
           ,[DIETARY_REQUIREMENT_NO]
           ,[SORT_NO]
           ,[ALLOW_REQUEST_YN]
           ,[APPROVED]
           ,[CHECKIN_YN]
           ,[CONTACT_TYPE]
           ,[GROUP_AVAILABLE_YN]
           ,[ICONF_REG_YN]
           ,[INTERNAL_STATUS]
           ,[INVITE_STATUS]
           ,[ONE_ON_ONE_YN]
           ,[PREFERRED_METHOD_OF_CONTACT]
           ,[PRESENTATION_YN]
           ,[PRINT_BADGE_YN]
           ,[REG_QUESTION_SEND_EMAIL_YN]
           ,[REG_STATUS]
           ,[REGISTRATION_TYPE]
           ,[SALES_SEND_EMAIL_YN]
           ,[SEND_EMAIL_YN]
           ,[WALK_IN_YN]
           ,[RR_ID]
           ,[REGISTRANT_CONTACT_ID]
           ,[COVERAGE_USER_ID]
           ,[INVITED_BY_USER_ID]
           ,[SCHEDULE_SENT_BY_USER_ID]
           ,[LOCALE_ID]
           ,[COVERAGE_USER_NAME]
           ,[INVITED_BY_USER_NAME]
           ,[REGISTRANT_CONT_FIRST_NAME]
           ,[REGISTRANT_CONT_LAST_NAME]
           ,[REGISTRANT_CONTACT_NAME]
           ,[REGISTRANT_CONTACT_NAME_UPPER]
           ,[RR_NAME]
           ,[SCHEDULE_SENT_BY_USER_NAME]
           ,[CREATED_DATE]
           ,[LAST_CHANGED_BY]
           ,[LAST_CHANGED_DATE]
           ,[CM_CONTACT_TYPE_NO]
           ,[INVITE_EMAIL_SENT_DATE]
           ,[LAST_BADGE_PRINT_DATE]
           ,[LAST_EMAIL_SENT_DATE]
           ,[LAST_INVITE_DATE]
           ,[LAST_REG_STATUS_DATE]
           ,[LAST_SCHEDULE_SENT_DATE]
           ,[BADGE_NAME]
           ,[EMAIL_ADDRESS]
           ,[TITLE]
           ,[PERSONAL_TITLE]
           ,[LAST_QUEST_RESPONSE_SENT_DATE]
           ,[ENTRY_SYSTEM_ID]
           ,[LAST_CHECKIN_CHANGED]
           ,[LAST_CHECKIN_CHANGED_BY]
           ,[LAST_AVAIL_CHANGED]
           ,[CM_CONTACT_CATEGORY_NO]
           ,[CONF_TARGET_ID]
           ,[PORTFOLIO_NO]
           ,[CORP_CLIENT_CONTACT_NO]
           ,[CAPITAL_MARKET_NO]
           ,[MARKET_TYPE]
           ,[CREATED_BY]
           ,[CONF_COMPANY_ID]
           ,[CM_TRANSLATION_TYPE_NO]
           ,[C_LEVEL_YN]
           ,[SYNC_COMPANY_ASSISTANTS_YN]
           ,[RIXML_CONTACT_ROLE_ID]
           ,[FIRST_REG_DATE]
           ,[LANGUAGES]
           ,[LANGUAGES_LC]
           ,[TRANSLATOR_YN]
           ,[VIRTUAL_ATTENDANCE_YN])
     VALUES
           (@confno
           ,(SELECT MAX([CONF_TARGET_NO]) + 1 FROM [CONF_TARGET] WHERE [CONF_NO] = @confno)
           ,1
           ,10
           ,'Y'
           ,'N'
           ,'N'
           ,'G'
           ,'N'
           ,'N'
           ,'I'
           ,'N'
           ,'N'
           ,'Y'
           ,'N'
           ,'N'
           ,'Y'
           ,'R'
           ,'I'
           ,'Y'
           ,'N'
           ,'N'
           ,''
           ,'90766'
           ,''
           ,''
           ,''
           ,'1033'
           ,''
           ,''
           ,'John'
           ,'Smith'
           ,'John Smith'
           ,'JOHN SMITH' 
           ,''
           ,''
           ,getdate()
           ,'ADMIN'
           ,getdate()
           ,1
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,getdate()
           ,NULL
           ,NULL
           ,@email
           ,''
           ,NULL
           ,NULL
           ,'CM'
           ,NULL
           ,NULL
           ,NULL
           ,0
           ,58997 --???
           ,NULL -- PROFILE_NO
           ,NULL
           ,NULL
           ,7
           ,'ADMIN'
           ,30984--????
           ,NULL
           ,'N'
           ,'N'
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,'N'
           ,NULL);


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

        public void AddTarget(int ConfId, IEnumerable<string> emails)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                foreach(var e in emails)
                {
                    SqlCommand cmd = new SqlCommand(insertsql, conn);

                    cmd.Parameters.Add(new SqlParameter("@confno", 875));
                    cmd.Parameters.Add(new SqlParameter("@email", 875));

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
