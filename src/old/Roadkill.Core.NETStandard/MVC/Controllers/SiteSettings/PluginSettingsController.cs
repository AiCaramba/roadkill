﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Plugins;
using Roadkill.Core.Security;
using Roadkill.Core.Services;

namespace Roadkill.Core.Mvc.Controllers
{
	[AdminRequired]
	public class PluginSettingsController : ControllerBase
	{
		private IPluginFactory _pluginFactory;
		private ISettingsRepository _settingsRepository;
		private SiteCache _siteCache;
		private PageViewModelCache _viewModelCache;
		private ListCache _listCache;

		public PluginSettingsController(ApplicationSettings settings, UserServiceBase userService, IUserContext context,
			SettingsService settingsService, IPluginFactory pluginFactory, ISettingsRepository settingsRepository, SiteCache siteCache,
			PageViewModelCache viewModelCache, ListCache listCache)
			: base(settings, userService, context, settingsService)
		{
			_pluginFactory = pluginFactory;
			_settingsRepository = settingsRepository;
			_siteCache = siteCache;
			_viewModelCache = viewModelCache;
			_listCache = listCache;
		}

		public ActionResult Index()
		{
			IEnumerable<TextPlugin> plugins = _pluginFactory.GetTextPlugins().OrderBy(x => x.Name);
			List<PluginViewModel> modelList = new List<PluginViewModel>();

			foreach (TextPlugin plugin in plugins)
			{
				modelList.Add(new PluginViewModel(plugin));
			}

			return View(modelList);
		}

		public ActionResult Edit(string id)
		{
			// Guards
			if (string.IsNullOrEmpty(id))
				return RedirectToAction("Index");

			TextPlugin plugin = _pluginFactory.GetTextPlugin(id);
			if (plugin == null)
				return RedirectToAction("Index");

			PluginViewModel model = new PluginViewModel()
			{
				Id = plugin.Id,
				DatabaseId = plugin.DatabaseId,
				Name = plugin.Name,
				Description = plugin.Description,
			};

			// Try to load the settings from the database, fall back to defaults
			model.SettingValues = new List<SettingValue>(plugin.Settings.Values);
			model.IsEnabled = plugin.Settings.IsEnabled;

			return View(model);
		}

		[HttpPost]
		public ActionResult Edit(PluginViewModel model)
		{
			TextPlugin plugin = _pluginFactory.GetTextPlugin(model.Id);
			if (plugin == null)
				return RedirectToAction("Index");

			// Update the plugin settings with the values from the summary
			plugin.Settings.IsEnabled = model.IsEnabled;

			foreach (SettingValue summaryValue in model.SettingValues)
			{
				SettingValue pluginValue = plugin.Settings.Values.FirstOrDefault(x => x.Name == summaryValue.Name);
				if (pluginValue != null)
					pluginValue.Value = summaryValue.Value;
			}

			// Update the plugin last saved date - this is important for 304 modified tracking
			// when the browser caching option is turned on.
			Configuration.SiteSettings settings = SettingsService.GetSiteSettings();
			settings.PluginLastSaveDate = DateTime.UtcNow;
			SettingsViewModel settingsViewModel = new SettingsViewModel(ApplicationSettings, settings);
			SettingsService.SaveSiteSettings(settingsViewModel);

			// Save and clear the cached settings
			_settingsRepository.SaveTextPluginSettings(plugin);
			_siteCache.RemovePluginSettings(plugin);

			// Clear all other caches if the plugin has been enabled or disabled.
			_viewModelCache.RemoveAll();
			_listCache.RemoveAll();

			return RedirectToAction("Index");
		}
	}
}