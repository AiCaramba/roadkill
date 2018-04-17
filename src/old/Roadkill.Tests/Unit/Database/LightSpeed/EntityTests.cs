﻿using System;
using NUnit.Framework;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Unit.Database.LightSpeed
{
	[TestFixture]
	public class EntityTests
	{
		[Test]
		public void user_objectid_should_match_id()
		{
			// Arrange
			User user = new User();
			user.Id = Guid.NewGuid();

			// Act
			Guid objectId = user.ObjectId;

			// Assert
			Assert.That(objectId, Is.EqualTo(user.Id));
		}

		[Test]
		public void pagecontent_objectid_should_match_id()
		{
			// Arrange
			PageContent page = new PageContent();
			page.ObjectId = Guid.NewGuid();

			// Act
			Guid objectId = page.ObjectId;

			// Assert
			Assert.That(objectId, Is.EqualTo(page.Id));
		}

		[Test]
		public void siteconfigurationentity_objectid_should_match_id()
		{
			// Arrange
			SiteConfigurationEntity siteConfigEntity = new SiteConfigurationEntity();
			siteConfigEntity.ObjectId = Guid.NewGuid();

			// Act
			Guid objectId = siteConfigEntity.ObjectId;

			// Assert
			Assert.That(objectId, Is.EqualTo(siteConfigEntity.Id));
		}
	}
}
