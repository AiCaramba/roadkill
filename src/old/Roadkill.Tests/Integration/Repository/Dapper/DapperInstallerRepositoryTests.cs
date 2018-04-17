﻿using System.Linq;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.Repositories.Dapper;
using Roadkill.Core.Database.Schema;

namespace Roadkill.Tests.Integration.Repository.Dapper
{
	[TestFixture]
	[Category("Integration")]
	public class DapperInstallerRepositoryTests : InstallerRepositoryTests
	{
		protected override string ConnectionString
		{
			get { return TestConstants.SQLSERVER_CONNECTION_STRING; }
		}

		protected override string InvalidConnectionString
		{
			get
			{
				return TestConstants.SQLSERVER_CONNECTION_STRING.Replace("Database=", "DatabaseInator=");
			}
		}

		private IDbConnectionFactory GetDbFactory(string connectionString = "")
		{
			if (string.IsNullOrEmpty(connectionString))
				connectionString = ConnectionString;

			return new SqlConnectionFactory(connectionString);
		}

		protected override IInstallerRepository GetRepository(string connectionString)
		{
			var schema = new SqlServerSchema();
			return new DapperInstallerRepository(GetDbFactory(connectionString), schema);
		}

		protected override void Clearup()
		{
			TestHelpers.SqlServerSetup.RecreateTables();
			TestHelpers.SqlServerSetup.ClearDatabase();
		}

		protected override bool HasEmptyTables()
		{
			IDbConnectionFactory factory = GetDbFactory();

			var settingsRepository = new DapperSettingsRepository(factory);
			var userRepository = new DapperUserRepository(factory);
			var pageRepository = new DapperPageRepository(factory);

			return pageRepository.AllPages().Count() == 0 &&
				   pageRepository.AllPageContents().Count() == 0 &&
				   userRepository.FindAllAdmins().Count() == 0 &&
				   userRepository.FindAllEditors().Count() == 0 &&
				   settingsRepository.GetSiteSettings() != null;
		}

		protected override bool HasAdminUser()
		{
			IDbConnectionFactory factory = GetDbFactory();
			var userRepository = new DapperUserRepository(factory);

			return userRepository.FindAllAdmins().Count() == 1;
		}

		protected override SiteSettings GetSiteSettings()
		{
			IDbConnectionFactory factory = GetDbFactory();
			var settingsRepository = new DapperSettingsRepository(factory);

			return settingsRepository.GetSiteSettings();
		}
	}
}
