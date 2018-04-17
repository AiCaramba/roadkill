﻿using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Email;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class EmailTemplateStub : EmailTemplate
	{
		public EmailTemplateStub(ApplicationSettings applicationSettings, ISettingsRepository settingsRepository, IEmailClient emailClient)
			: base(applicationSettings, settingsRepository, emailClient)
		{
			base.PlainTextView = "plaintextview";
			base.HtmlView = "htmlview";
		}
		
		public override void Send(UserViewModel model)
		{
			base.Send(model);
		}

		public IEmailClient GetEmailClient()
		{
			return base.EmailClient;
		}

		public SiteSettings GetSiteSettings()
		{
			return SiteSettings;
		}
	}
}
