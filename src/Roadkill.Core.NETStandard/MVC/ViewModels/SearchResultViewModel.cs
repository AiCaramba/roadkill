﻿using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Roadkill.Core.Mvc.ViewModels
{
	/// <summary>
	/// Contains data for a single search result from a search query.
	/// </summary>
	public class SearchResultViewModel
	{
		/// <summary>
		/// The page id
		/// </summary>
		public int Id { get; internal set; }

		/// <summary>
		/// The page title.
		/// </summary>
		public string Title { get; internal set; }

		/// <summary>
		/// The page title, encoded so it is a safe search-engine friendly url.
		/// </summary>
		public string EncodedTitle
		{
			get
			{
				return PageViewModel.EncodePageTitle(Title);
			}
		}

		/// <summary>
		/// The summary of the content (the first 150 characters of text with all HTML removed).
		/// </summary>
		public string ContentSummary { get; internal set; }

		/// <summary>
		/// The length of the content in bytes.
		/// </summary>
		public int ContentLength { get; internal set; }

		// TODO: tests
		/// <summary>
		/// Formats the page length in bytes using KB or bytes if it is less than 1024 bytes.
		/// </summary>
		/// <param name="helper">The helper.</param>
		/// <param name="size">The size in bytes.</param>
		/// <returns>If the size parameter is 900: 900 bytes. If size is 4000: 4KB.</returns>
		public string ContentLengthInKB
		{
			get
			{
				if (ContentLength > 1024)
					return ContentLength / 1024 + "KB";
				else
					return ContentLength + " bytes";
			}
		}

		/// <summary>
		/// The person who created the page.
		/// </summary>
		public string CreatedBy { get; internal set; }

		/// <summary>
		/// The date the page was created on.
		/// </summary>
		public DateTime CreatedOn { get; internal set; }

		/// <summary>
		/// The tags for the page, in space delimited format.
		/// </summary>
		public string Tags { get; internal set; }

		/// <summary>
		/// The lucene.net score for the search result.
		/// </summary>
		public float Score { get; internal set; }

		public SearchResultViewModel()
		{
		}

		public SearchResultViewModel(Document document, ScoreDoc scoreDoc)
		{
			if (document == null)
				throw new ArgumentNullException(nameof(document));

			if (scoreDoc == null)
				throw new ArgumentNullException(nameof(scoreDoc));

			EnsureFieldsExist(document);

			Id = int.Parse(document.GetField("id").GetStringValue());
			Title = document.GetField("title").GetStringValue();
			ContentSummary = document.GetField("contentsummary").GetStringValue();
			Tags = document.GetField("tags").GetStringValue();
			CreatedBy = document.GetField("createdby").GetStringValue();
			ContentLength = int.Parse(document.GetField("contentlength").GetStringValue());
			Score = scoreDoc.Score;

			DateTime createdOn = DateTime.UtcNow;
			if (!DateTime.TryParse(document.GetField("createdon").GetStringValue(), out createdOn))
				createdOn = DateTime.UtcNow;

			CreatedOn = createdOn;
		}

		private void EnsureFieldsExist(Document document)
		{
			IList<IIndexableField> fields = document.Fields;
			EnsureFieldExists(fields, "id");
			EnsureFieldExists(fields, "title");
			EnsureFieldExists(fields, "contentsummary");
			EnsureFieldExists(fields, "tags");
			EnsureFieldExists(fields, "createdby");
			EnsureFieldExists(fields, "contentlength");
			EnsureFieldExists(fields, "createdon");
		}

		private void EnsureFieldExists(IList<IIndexableField> fields, string fieldname)
		{
			if (fields.Any(x => x.Name == fieldname) == false)
				throw new SearchException(null, "The LuceneDocument did not contain the expected field '{0}'", fieldname);
		}

		/// <summary>
		/// Used by the search results view, for the list of tags.
		/// </summary>
		public IEnumerable<string> TagsAsList()
		{
			List<string> tags = new List<string>();

			if (!string.IsNullOrEmpty(Tags))
			{
				if (Tags.IndexOf(" ") != -1)
				{
					string[] parts = Tags.Split(' ');
					foreach (string item in parts)
					{
						if (item != " ")
							tags.Add(item);
					}
				}
				else
				{
					tags.Add(Tags.TrimEnd());
				}
			}

			return tags;
		}
	}
}