﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Mvc.WebApi;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Mvc.WebApi
{
	[TestFixture]
	[Category("Unit")]
	public class PageControllerTests
	{
		private MocksAndStubsContainer _container;

		private PageRepositoryMock _pageRepositoryMock;
		private PageService _pageService;
		private PagesController _pagesController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_pageRepositoryMock = _container.PageRepository;
			_pageService = _container.PageService;

			_pagesController = new PagesController(_pageService);
		}

		[Test]
		public void get_should_return_all_pages()
		{
			// Arrange
			_pageService.AddPage(new PageViewModel() { Id = 1, Title = "new page" });
			_pageService.AddPage(new PageViewModel() { Id = 2, Title = "new page", IsLocked = true });

			// Act
			IEnumerable<PageViewModel> pages = _pagesController.Get();

			// Assert
			Assert.That(pages.Count(), Is.EqualTo(2));
		}

		[Test]
		public void get_should_return_page_by_id()
		{
			// Arrange
			Page expectedPage = new Page() { Id = 7, Title = "new page" };
			_pageRepositoryMock.Pages.Add(expectedPage);

			// Act
			PageViewModel actualPage = _pagesController.Get(7);

			// Assert
			Assert.That(actualPage, Is.Not.Null);
			Assert.That(actualPage.Id, Is.EqualTo(expectedPage.Id));
		}

		[Test]
		public void get_should_return_null_when_page_does_not_exist()
		{
			// Arrange

			// Act
			PageViewModel actualPage = _pagesController.Get(99);

			// Assert
			Assert.That(actualPage, Is.Null);
		}

		[Test]
		public void put_should_update_page()
		{
			// Arrange
			DateTime version1Date = DateTime.Today.AddDays(-1); // stops the getlatestcontent acting up when add+update are the same time

			Page page = new Page();
			page.Title = "Hello world";
			page.Tags = "tag1, tag2";
			page.CreatedOn = version1Date;
			page.ModifiedOn = version1Date;
			PageContent pageContent = _pageRepositoryMock.AddNewPage(page, "Some content1", "editor", version1Date);

			PageViewModel model = new PageViewModel(pageContent.Page);
			model.Title = "New title";
			model.Content = "Some content2";
			model.ModifiedOn = DateTime.UtcNow;

			// Act
			_pagesController.Put(model);

			// Assert
			 Assert.That(_pageService.AllPages().Count(), Is.EqualTo(1));

			PageViewModel actualPage = _pageService.GetById(1, true);
			Assert.That(actualPage.Title, Is.EqualTo("New title"));
			Assert.That(actualPage.Content, Is.EqualTo("Some content2"));
		}

		[Test]
		public void post_should_add_page()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.Title = "Hello world";
			model.RawTags = "tag1, tag2";
			model.Content = "Some content";

			// Act
			_pagesController.Post(model);

			// Assert
			Assert.That(_pageService.AllPages().Count(), Is.EqualTo(1));
		}
	}
}