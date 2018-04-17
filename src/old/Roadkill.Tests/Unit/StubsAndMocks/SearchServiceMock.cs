﻿using System.Collections.Generic;
using System.Linq;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Plugins;
using Roadkill.Core.Services;
using Roadkill.Core.Text;
using Roadkill.Core.Text.TextMiddleware;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class SearchServiceMock : SearchService
	{
		public List<Page> Pages { get; set; }
		public List<PageContent> PageContents { get; set; }
		public bool CreatedNewIndex { get; set; }

		public SearchServiceMock(ApplicationSettings settings, ISettingsRepository settingsRepository, IPageRepository pageRepository, TextMiddlewareBuilder textMiddlewareBuilder)
			: base(settings, settingsRepository, pageRepository, textMiddlewareBuilder)
		{
			Pages = new List<Page>();
			PageContents = new List<PageContent>();
		}

		public override void Add(PageViewModel model)
		{	
		}

		public override void Update(PageViewModel model)
		{	
		}

		public override void CreateIndex()
		{
			CreatedNewIndex = true;
		}

		public override int Delete(PageViewModel model)
		{
			return 1;
		}

		public override IEnumerable<SearchResultViewModel> Search(string searchText)
		{
			List<SearchResultViewModel> results = new List<SearchResultViewModel>();

			foreach (Page page in Pages.Where(p => p.Title.ToLowerInvariant().Contains(searchText.ToLowerInvariant())))
			{
				results.Add(new SearchResultViewModel()
				{
					Id = page.Id,
					Title = page.Title,
					ContentSummary = PageContents.Single(p => p.Page == page).Text
				});
			}

			foreach (PageContent pageContent in PageContents.Where(p => p.Text.ToLowerInvariant().Contains(searchText.ToLowerInvariant())))
			{
				results.Add(new SearchResultViewModel()
				{
					Id = pageContent.Page.Id,
					ContentSummary = pageContent.Text,
					Title = pageContent.Page.Title
				});
			}

			return results;
		}
	}
}
