﻿using System;
using NUnit.Framework;
using Roadkill.Core.Text.Parsers.Images;
using Roadkill.Core.Text.Parsers.Links;
using Roadkill.Core.Text.Parsers.Markdig;

namespace Roadkill.Tests.Unit.Text.Parsers.Markdig
{
    public class MarkdigParserTests
    {
		[Test]
		public void should_ignore_null_imageparsed_and_linkparsed_funcs()
		{
			// given
			string expectedHtml = "<p><a href=\"http://www.google.com\" rel=\"nofollow\">googz</a>"+
									"<img src=\"/myimage.jpg\" class=\"img-responsive\" border=\"0\" alt=\"img\" title=\"img\" /></p>\n";
			var parser = new MarkdigConverter();

			// when
			string actualHtml = parser.ToHtml("[googz](http://www.google.com)![img](/myimage.jpg)");

			// then
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_bind_imageparsed_and_linkparsed_funcs()
		{
			// given
			Func<HtmlImageTag, HtmlImageTag> imageParsed = (imageTag) =>
			{
				imageTag.Title = "new title";
				imageTag.Alt = "new alt";
				imageTag.Src = "/new src";
				return imageTag;
			}; 

			Func<HtmlLinkTag, HtmlLinkTag> linkParsed = (linkTag) =>
			{
				linkTag.CssClass = "new css";
				linkTag.Href = "/new href";
				linkTag.Text = "new text";

				return linkTag;
			};

			string expectedHtml = "<p><a href=\"/new%20href\" class=\"new css\">new text</a>" +
									"<img src=\"/new%20src\" class=\"img-responsive\" border=\"0\" alt=\"new alt\" title=\"new title\" /></p>\n";
			var parser = new MarkdigConverter();
			parser.ImageParsed = imageParsed;
			parser.LinkParsed = linkParsed;

			// when
			string actualHtml = parser.ToHtml("[googz](http://www.google.com)![img](/myimage.jpg)");

			// then
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
	    public void should_retain_anchor_tags_and_querystrings()
	    {
			// Arrange
			string expectedHtml = "<p><a href=\"?a=%23myvalue#myanchortag\">hello world</a></p>\n";
			string markdown = "[hello world](?a=%23myvalue#myanchortag)";
			var parser = new MarkdigConverter();

			// when
			string html = parser.ToHtml(markdown);

			// then
			Assert.That(html, Is.EqualTo(expectedHtml));
		}

        [Test]
        public void should_handle_empty_strings()
        {
			// given
			var parser = new MarkdigConverter();

			// when
			string html = parser.ToHtml(null);

			// then
			Assert.That(html, Is.EqualTo(""));
		}

        [Test]
        public void should_parse_basic_markdown()
        {
            // given
            string expectedHtml = "<p><a href=\"http://www.google.com\" class=\"main\" rel=\"nofollow\">i am a link</a></p>\n";
            string markdown = "[i am a link](http://www.google.com){.main}";
            var parser = new MarkdigConverter();

            // when
            string html = parser.ToHtml(markdown);

            // then
            Assert.That(html, Is.EqualTo(expectedHtml));
        }

	    [Test]
	    public void should_call_image_parsed_func()
	    {
		    Assert.Fail("Fail");
	    }

		[Test]
		public void should_call_link_parsed_func()
		{
			Assert.Fail("Fail");
		}

		[Test]
		public void should_handle_advanced_markdown()
		{
			Assert.Fail("Fail");
		}
	}
}
