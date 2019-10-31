﻿using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.Misc.ApplicationInsights.Models;
using Nop.Plugin.Misc.ApplicationInsights.Helpers;
using Nop.Services.Messages;
using System;

namespace Nop.Plugin.Misc.ApplicationInsights.Controllers
{
    public class MiscApplicationInsightsController : BasePluginController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public MiscApplicationInsightsController(ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService)
        {
            this._settingService = settingService;
            this._notificationService = notificationService;
            this._localizationService = localizationService;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin] //confirms access to the admin panel
        [Area(AreaNames.Admin)] //specifies the area containing a controller or action
        public IActionResult Configure()
        {
            var config = ConfigHelper.GetConfig();

            string instrumentationKey = config["ApplicationInsights:InstrumentationKey"];
            bool enableQuickPulseMetricStream = Convert.ToBoolean(config["ApplicationInsights:Settings:EnableQuickPulseMetricStream"]);
            bool enableAdaptiveSampling = Convert.ToBoolean(config["ApplicationInsights:Settings:EnableAdaptiveSampling"]);
            bool enableHeartbeat = Convert.ToBoolean(config["ApplicationInsights:Settings:EnableHeartbeat"]);
            bool addAutoCollectedMetricExtractor = Convert.ToBoolean(config["ApplicationInsights:Settings:AddAutoCollectedMetricExtractor"]);

            var model = new ConfigurationModel
            {
                InstrumentationKey = instrumentationKey,
                EnableQuickPulseMetricStream = enableQuickPulseMetricStream,
                EnableAdaptiveSampling = enableAdaptiveSampling,
                EnableHeartbeat = enableHeartbeat,
                AddAutoCollectedMetricExtractor = addAutoCollectedMetricExtractor
            };

            return View("~/Plugins/Misc.ApplicationInsights/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            // String that represents the new JSON with updated Application Insights values from the model
            string appSettings = $"{{\r\n\t\"ApplicationInsights\": {{\r\n\t\t\"InstrumentationKey\": \"{model.InstrumentationKey}\",\r\n\t\t\"Settings\": {{\r\n\t\t\t\"EnableQuickPulseMetricStream\": \"{model.EnableQuickPulseMetricStream}\",\r\n\t\t\t\"EnableAdaptiveSampling\": \"{model.EnableAdaptiveSampling}\",\r\n\t\t\t\"EnableHeartbeat\": \"{model.EnableHeartbeat}\",\r\n\t\t\t\"AddAutoCollectedMetricExtractor\": \"{model.AddAutoCollectedMetricExtractor}\"\r\n\t\t}}\r\n\t}}\r\n}}";

            // Overwrite appsettings.json file with the new JSONs
            System.IO.File.WriteAllText($"{ConfigHelper.GetConfigDirectory()}\\appsettings.json", appSettings);

            // Now clear settings cache
            //_settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }



        #endregion
    }
}