﻿using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Core.Database.MongoDB
{
	public class MongoDBPageRepository : IPageRepository
	{
		internal readonly string ConnectionString;

		public IQueryable<Page> Pages => Queryable<Page>();
		public IQueryable<PageContent> PageContents => Queryable<PageContent>();

		public MongoDBPageRepository(string connectionString)
		{
			ConnectionString = connectionString;
		}

		public void Wipe()
		{
			string databaseName = MongoUrl.Create(ConnectionString).DatabaseName;
			MongoClient client = new MongoClient(ConnectionString);
			IMongoDatabase database = client.GetDatabase(databaseName, new MongoDatabaseSettings());

			database.DropCollection(nameof(PageContent));
			database.DropCollection(nameof(Page));
			database.DropCollection(nameof(User));
			database.DropCollection(nameof(SiteConfigurationEntity));
		}

		private IMongoCollection<T> GetCollection<T>()
		{
			string connectionString = ConnectionString;

			string databaseName = MongoUrl.Create(ConnectionString).DatabaseName;
			MongoClient client = new MongoClient(ConnectionString);
			IMongoDatabase database = client.GetDatabase(databaseName, new MongoDatabaseSettings());

			return database.GetCollection<T>(typeof(T).Name);
		}

		public void Delete<T>(T obj) where T : IDataStoreEntity
		{
			IMongoCollection<T> collection = GetCollection<T>();
			collection.FindOneAndDelete(x => x.ObjectId == obj.ObjectId);
		}

		public void DeleteAll<T>() where T : IDataStoreEntity
		{
			IMongoCollection<T> collection = GetCollection<T>();
			collection.DeleteMany(x => true);
		}

		public IQueryable<T> Queryable<T>() where T : IDataStoreEntity
		{
			return GetCollection<T>().AsQueryable();
		}

		public void SaveOrUpdate<T>(T obj) where T : IDataStoreEntity
		{
			// Implement autoincrement identity(1,1) for MongoDB, for Page objects
			Page page = obj as Page;
			if (page != null && page.Id == 0)
			{
				int newId = 1;
				Page recentPage = Queryable<Page>().OrderByDescending(x => x.Id).FirstOrDefault();
				if (recentPage != null)
				{
					newId = recentPage.Id + 1;
				}

				obj.ObjectId = Guid.NewGuid();
				page.Id = newId;
			}

			IMongoCollection<T> collection = GetCollection<T>();
			collection.FindOneAndReplace<T>(x => x.ObjectId == obj.ObjectId, obj);
		}

		public PageContent GetLatestPageContent(int pageId)
		{
			return Queryable<PageContent>()
				.Where(x => x.Page.Id == pageId)
				.OrderByDescending(x => x.EditedOn)
				.FirstOrDefault();
		}

		public IEnumerable<Page> AllPages()
		{
			return Pages.ToList();
		}

		public Page GetPageById(int id)
		{
			return Pages.FirstOrDefault(p => p.Id == id);
		}

		public IEnumerable<Page> FindPagesCreatedBy(string username)
		{
			return Pages.Where(p => p.CreatedBy == username);
		}

		public IEnumerable<Page> FindPagesModifiedBy(string username)
		{
			return Pages.Where(p => p.ModifiedBy == username);
		}

		public IEnumerable<Page> FindPagesContainingTag(string tag)
		{
			return Pages.Where(p => p.Tags.ToLower().Contains(tag.ToLower()));
		}

		public IEnumerable<string> AllTags()
		{
			return new List<string>(Pages.Select(p => p.Tags));
		}

		public Page GetPageByTitle(string title)
		{
			if (string.IsNullOrEmpty(title))
				return null;

			return Pages.FirstOrDefault(p => p.Title == title);
		}

		public PageContent GetPageContentById(Guid id)
		{
			return PageContents.FirstOrDefault(p => p.Id == id);
		}

		public PageContent GetPageContentByPageIdAndVersionNumber(int id, int versionNumber)
		{
			return PageContents.FirstOrDefault(p => p.Page.Id == id && p.VersionNumber == versionNumber);
		}

		public IEnumerable<PageContent> FindPageContentsByPageId(int pageId)
		{
			return PageContents.Where(p => p.Page.Id == pageId);
		}

		public IEnumerable<PageContent> AllPageContents()
		{
			return PageContents.ToList();
		}

		public IEnumerable<PageContent> FindPageContentsEditedBy(string username)
		{
			return PageContents.Where(p => p.EditedBy == username);
		}

		public void DeletePage(Page page)
		{
			Delete<Page>(page);
		}

		public void DeletePageContent(PageContent pageContent)
		{
			Delete<PageContent>(pageContent);
		}

		public void DeleteAllPages()
		{
			DeleteAll<Page>();
			DeleteAll<PageContent>();
		}

		public Page SaveOrUpdatePage(Page page)
		{
			SaveOrUpdate<Page>(page);
			return page;
		}

		public PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn)
		{
			SaveOrUpdate<Page>(page);

			PageContent pageContent = new PageContent()
			{
				Id = Guid.NewGuid(),
				Page = page,
				Text = text,
				EditedBy = editedBy,
				EditedOn = editedOn,
				VersionNumber = 1,
			};

			SaveOrUpdate<PageContent>(pageContent);
			return pageContent;
		}

		public PageContent AddNewPageContentVersion(Page page, string text, string editedBy, DateTime editedOn, int version)
		{
			page.ModifiedOn = editedOn;
			page.ModifiedBy = editedBy;
			SaveOrUpdate<Page>(page);

			PageContent pageContent = new PageContent()
			{
				Id = Guid.NewGuid(),
				Page = page,
				Text = text,
				EditedBy = editedBy,
				EditedOn = editedOn,
				VersionNumber = version,
			};

			SaveOrUpdate<PageContent>(pageContent);

			return pageContent;
		}

		public void UpdatePageContent(PageContent content)
		{
			SaveOrUpdate<PageContent>(content);
		}

		public void Dispose()
		{
		}
	}
}