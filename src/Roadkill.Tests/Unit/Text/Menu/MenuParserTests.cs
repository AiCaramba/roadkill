//using System;
//using Xunit;
//using Roadkill.Core.Cache;
//using Roadkill.Core.Configuration;
//using Roadkill.Core.Database;
//using Roadkill.Core.Text;
//using Roadkill.Core.Text.Menu;
//using Roadkill.Core.Text.Parsers.Markdig;
//using Roadkill.Tests.Unit.StubsAndMocks;

//namespace Roadkill.Tests.Unit.Text.Menu
//{
//	public class MenuParserTests
//	{
//		private PageRepositoryMock _pageRepository;
//		private SettingsRepositoryMock _settingsRepository;
//		private UserContextStub _userContext;
//		private ApplicationSettings _applicationSettings;
//		private CacheMock _cache;
//		private SiteCache _siteCache;
//		private MenuParser _menuParser;
//	    private MarkdigParser _markupParser;

//		public MenuParserTests
//()
//		{
//			new PluginFactoryMock();

//			_pageRepository = new PageRepositoryMock();

//			_settingsRepository = new SettingsRepositoryMock();
//			_settingsRepository.SiteSettings = new SiteSettings();

//			_userContext = new UserContextStub();

//			_applicationSettings = new ApplicationSettings();
//			_applicationSettings.Installed = true;

//			_cache = new CacheMock();
//			_siteCache = new SiteCache(_cache);

//	        _markupParser = new MarkdigParser();
//            _menuParser = new MenuParser(_markupParser, _settingsRepository, _siteCache, _userContext);
//		}

//		[Fact]
//		public void should_replace_known_tokens_when_logged_in_as_admin()
//		{
//			// Arrange
//			string menuMarkup = "* %categories%\r\n\r\n%allpages%\r\n%mainpage%\r\n%newpage%\r\n%managefiles%\r\n%sitesettings%\r\n";
//			string expectedHtml = "<ul><li><a href=\"/pages/alltags\">Categories</a></li></ul>" +
//								  "<a href=\"/pages/allpages\">All pages</a>" +
//								  "<a href=\"/\">Main Page</a>" +
//								  "<a href=\"/pages/new\">New page</a>" +
//								  "<a href=\"/filemanager\">Manage files</a>" +
//								  "<a href=\"/settings\">Site settings</a>";

//			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

//			_userContext.IsAdmin = true;
//			_userContext.IsLoggedIn = true;

//			// Act
//			string actualHtml = _menuParser.GetMenu();

//			// Assert
//			Assert.Equal(, actualHtml);
//		}

//		[Fact]
//		public void should_replace_known_tokens_when_logged_in_as_editor()
//		{
//			// Arrange
//			string menuMarkup = "* %categories%\r\n\r\n%allpages%\r\n%mainpage%\r\n%newpage%\r\n%managefiles%\r\n%sitesettings%\r\n";
//			string expectedHtml = "<ul><li><a href=\"/pages/alltags\">Categories</a></li></ul>" +
//								  "<a href=\"/pages/allpages\">All pages</a>" +
//								  "<a href=\"/\">Main Page</a>" +
//								  "<a href=\"/pages/new\">New page</a><a href=\"/filemanager\">Manage files</a>";

//			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

//			_userContext.IsAdmin = false;
//			_userContext.IsLoggedIn = true;

//			// Act
//			string actualHtml = _menuParser.GetMenu();

//			// Assert
//			Assert.Equal(, actualHtml);
//		}

//		[Fact]
//		public void should_replace_known_tokens_when_not_logged()
//		{
//			// Arrange
//			string menuMarkup = "* %categories%\r\n\r\n%allpages%\r\n%mainpage%\r\n%newpage%\r\n%managefiles%\r\n%sitesettings%\r\n";
//			string expectedHtml = "<ul><li><a href=\"/pages/alltags\">Categories</a></li></ul>" +
//								  "<a href=\"/pages/allpages\">All pages</a>" +
//								  "<a href=\"/\">Main Page</a>";

//			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

//			// Act
//			string actualHtml = _menuParser.GetMenu();

//			// Assert
//			Assert.Equal(, actualHtml);
//		}

//		[Theory]
//	[InlineData("<a href=\"/\">Main Page</a>")]
//		public void Should_Remove_Empty_UL_Tags_For_Logged_In_Tokens_When_Not_Logged_In(string expectedHtml)
//		{
//			// Arrange - \r\n is important so the markdown is valid
//			string menuMarkup = "%mainpage%\r\n\r\n* %newpage%\r\n* %managefiles%\r\n* %sitesettings%\r\n";
//			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

//			// Act
//			string actualHtml = _menuParser.GetMenu();

//			// Assert
//			Assert.Equal(expectedHtml, actualHtml);
//		}

//		[Fact]
//		public void should_cache_menu_html_for_admin_and_editor_and_guest_user()
//		{
//			// Arrange
//			string menuMarkup = "My menu %newpage% %sitesettings%";
//			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

//			// Act
//			_userContext.IsLoggedIn = false;
//			_userContext.IsAdmin = false;
//			_menuParser.GetMenu();

//			_userContext.IsLoggedIn = true;
//			_userContext.IsAdmin = false;
//			_menuParser.GetMenu();

//			_userContext.IsLoggedIn = true;
//			_userContext.IsAdmin = true;
//			_menuParser.GetMenu();

//			// Assert
//			Assert.Equal(3, _cache.CacheItems.Count);
//		}

//		[Fact]
//		public void should_return_different_menu_html_for_admin_and_editor_and_guest_user()
//		{
//			// Arrange
//			string menuMarkup = "My menu %newpage% %sitesettings%";
//			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

//			// Act
//			_userContext.IsLoggedIn = false;
//			_userContext.IsAdmin = false;
//			string guestHtml = _menuParser.GetMenu();

//			_userContext.IsLoggedIn = true;
//			_userContext.IsAdmin = false;
//			string editorHtml = _menuParser.GetMenu();

//			_userContext.IsLoggedIn = true;
//			_userContext.IsAdmin = true;
//			string adminHtml = _menuParser.GetMenu();

//			// Assert
//			Assert.Equal("My menu", guestHtml);
//			Assert.Equal("My menu <a href=\"/pages/new\">New page</a>", editorHtml);
//			Assert.Equal("My menu <a href=\"/pages/new\">New page</a> <a href=\"/settings\">Site settings</a>", adminHtml);
//		}

//		[Fact]
//		public void should_replace_markdown_with_external_link()
//		{
//			// Arrange
//			string menuMarkup = "* [First link](http://www.google.com)\r\n";
//			string expectedHtml = "<ul><li><a href=\"http://www.google.com\" rel=\"nofollow\">First link</a></li></ul>";
//			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

//			// Act
//			string actualHtml = _menuParser.GetMenu();

//			// Assert
//			Assert.Equal(, actualHtml);
//		}

//		[Fact]
//		public void should_replace_markdown_with_internal_link()
//		{
//			// Arrange
//			string menuMarkup = "* [First link](my-page)";
//			string expectedHtml = "<ul><li><a href=\"my-page\">First link</a></li></ul>";
//			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

//            _pageRepository.AddNewPage(new Page() { Title = "my page", Id = 1 }, "text", "user", DateTime.Now);

//			// Act
//			string actualHtml = _menuParser.GetMenu();

//			// Assert
//			Assert.Equal(, actualHtml);
//		}
//	}
//}