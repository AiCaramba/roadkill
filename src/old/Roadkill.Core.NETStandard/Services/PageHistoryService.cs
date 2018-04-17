﻿using System;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Text.Menu;
using Roadkill.Core.Text.TextMiddleware;

namespace Roadkill.Core.Services
{
	/// <summary>
	/// Provides a way of viewing, and comparing the version history of page content, and reverting to previous versions.
	/// </summary>
	public class PageHistoryService
	{
		private readonly IUserContext _context;
		private readonly PageViewModelCache _pageViewModelCache;
		private readonly TextMiddlewareBuilder _textMiddlewareBuilder;

		public ApplicationSettings ApplicationSettings { get; set; }
		public ISettingsRepository SettingsRepository { get; set; }
		public IPageRepository PageRepository { get; set; }

		public PageHistoryService(ISettingsRepository settingsRepository, IPageRepository pageRepository, IUserContext context,
			PageViewModelCache pageViewModelCache, TextMiddlewareBuilder textMiddlewareBuilder)
		{
			_context = context;
			_pageViewModelCache = pageViewModelCache;
			_textMiddlewareBuilder = textMiddlewareBuilder;

			SettingsRepository = settingsRepository;
			PageRepository = pageRepository;
		}

		/// <summary>
		/// Retrieves all history for a page.
		/// </summary>
		/// <param name="pageId">The id of the page to get the history for.</param>
		/// <returns>An <see cref="IEnumerable{PageHistoryViewModel}"/> ordered by the most recent version number.</returns>
		/// <exception cref="HistoryException">An database error occurred while retrieving the list.</exception>
		public IEnumerable<PageHistoryViewModel> GetHistory(int pageId)
		{
			try
			{
				IEnumerable<PageContent> contentList = PageRepository.FindPageContentsByPageId(pageId);
				IEnumerable<PageHistoryViewModel> historyList = from p in contentList
																select new PageHistoryViewModel(p);

				return historyList.OrderByDescending(h => h.VersionNumber);
			}
			catch (ArgumentNullException ex)
			{
				throw new HistoryException(ex, "An ArgumentNullException occurred getting the history for page id {0}", pageId);
			}
			catch (DatabaseException ex)
			{
				throw new HistoryException(ex, "A DatabaseException occurred getting the history for page id {0}", pageId);
			}
		}

		/// <summary>
		/// Compares a page version to the previous version.
		/// </summary>
		/// <param name="mainVersionId">The id of the version to compare</param>
		/// <returns>Returns a IEnumerable of two versions, where the 2nd item is the previous version.
		/// If the current version is 1, or a previous version cannot be found, then the 2nd item will be null.</returns>
		/// <exception cref="HistoryException">An database error occurred while comparing the two versions.</exception>
		public IEnumerable<PageViewModel> CompareVersions(Guid mainVersionId)
		{
			try
			{
				List<PageViewModel> versions = new List<PageViewModel>();

				PageContent mainContent = PageRepository.GetPageContentById(mainVersionId);
				PageHtml html = _textMiddlewareBuilder.Execute(mainContent.Text);

				versions.Add(new PageViewModel(mainContent, html));

				if (mainContent.VersionNumber == 1)
				{
					versions.Add(null);
				}
				else
				{
					PageViewModel model = _pageViewModelCache.Get(mainContent.Page.Id, mainContent.VersionNumber - 1);

					if (model == null)
					{
						PageContent previousContent = PageRepository.GetPageContentByPageIdAndVersionNumber(mainContent.Page.Id, mainContent.VersionNumber - 1);
						if (previousContent == null)
						{
							model = null;
						}
						else
						{
							PageContent pageContent = PageRepository.GetLatestPageContent(previousContent.Page.Id);
							html = _textMiddlewareBuilder.Execute(pageContent.Text);
							model = new PageViewModel(previousContent, html);

							_pageViewModelCache.Add(mainContent.Page.Id, mainContent.VersionNumber - 1, model);
						}
					}

					versions.Add(model);
				}

				return versions;
			}
			catch (ArgumentNullException ex)
			{
				throw new HistoryException(ex, "An ArgumentNullException occurred comparing the version history for version id {0}", mainVersionId);
			}
			catch (DatabaseException ex)
			{
				throw new HistoryException(ex, "A HibernateException occurred comparing the version history for version id {0}", mainVersionId);
			}
		}

		/// <summary>
		/// Reverts a page to a particular version, creating a new version in the process.
		/// </summary>
		/// <param name="pageId">The id of the page</param>
		/// <param name="versionNumber">The version number to revert to.</param>
		/// <exception cref="HistoryException">An databaseerror occurred while reverting to the version.</exception>
		public void RevertTo(int pageId, int versionNumber)
		{
			try
			{
				PageContent pageContent = PageRepository.GetPageContentByPageIdAndVersionNumber(pageId, versionNumber);

				if (pageContent != null)
				{
					RevertTo(pageContent.Id, _context);
				}
			}
			catch (ArgumentNullException ex)
			{
				throw new HistoryException(ex, "An ArgumentNullException occurred when reverting to version number {0} for page id {1}", versionNumber, pageId);
			}
			catch (DatabaseException ex)
			{
				throw new HistoryException(ex, "A DatabaseException occurred when reverting to version number {0} for page id {1}", versionNumber, pageId);
			}
		}

		/// <summary>
		/// Reverts to a particular version, creating a new version in the process.
		/// </summary>
		/// <param name="versionId">The version ID to revert to.</param>
		/// <param name="context">The current logged in user's context.</param>
		/// <exception cref="HistoryException">An databaseerror occurred while reverting to the version.</exception>
		public void RevertTo(Guid versionId, IUserContext context)
		{
			try
			{
				string currentUser = context.CurrentUsername;

				PageContent versionContent = PageRepository.GetPageContentById(versionId);
				Page page = PageRepository.GetPageById(versionContent.Page.Id);

				int versionNumber = MaxVersion(page.Id) + 1;
				string text = versionContent.Text;
				string editedBy = currentUser;
				DateTime editedOn = DateTime.UtcNow;
				PageRepository.AddNewPageContentVersion(page, text, editedBy, editedOn, versionNumber);

				// Clear the cache
				_pageViewModelCache.Remove(page.Id);
			}
			catch (ArgumentNullException ex)
			{
				throw new HistoryException(ex, "An ArgumentNullException occurred when reverting to version ID {0}", versionId);
			}
			catch (DatabaseException ex)
			{
				throw new HistoryException(ex, "A DatabaseException occurred when reverting to version ID {0}", versionId);
			}
		}

		/// <summary>
		/// Retrieves the latest version number for a page.
		/// </summary>
		/// <param name="pageId">The id of the page to get the version number for.</param>
		/// <returns>The latest version number.</returns>
		public int MaxVersion(int pageId)
		{
			return PageRepository.GetLatestPageContent(pageId).VersionNumber;
		}
	}
}