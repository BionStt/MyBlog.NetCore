﻿using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using MyExtensionsLib;
using MyBlog.Core;
using MyBlog.Core.ViewProjections.Home;
using MyBlog.Web.Common;
using Microsoft.Extensions.Options;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MyBlog.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMemoryCache _memoryCache;
        private readonly IViewProjectionFactory _viewProjectionFactory;
        private readonly ICommandInvokerFactory _commandInvokerFactory;
        private readonly IOptions<AppConfig> _appConfig;

        public HomeController(
            IHostingEnvironment hostingEnvironment,
            IMemoryCache memoryCache,
            IViewProjectionFactory viewProjectionFactory,
            ICommandInvokerFactory commandInvokerFactory,
            IOptions<AppConfig> appConfig)
        {
            this._hostingEnvironment = hostingEnvironment;
            this._memoryCache = memoryCache;
            this._viewProjectionFactory = viewProjectionFactory;
            this._commandInvokerFactory = commandInvokerFactory;
            this._appConfig = appConfig;
        }


        #region 首页

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(string page)
        {
            if (!int.TryParse(page, out int pageNum))
            {
                pageNum = 1;
            }
            var resModel = this._viewProjectionFactory.GetViewProjection<HomeBindModel, HomeViewModel>(new HomeBindModel() { PageNum = pageNum });
            Set();
            return View("Index", resModel);
        }


        #endregion



        #region 根据博客编号，时间查询博客详细信息

        /// <summary>
        /// 博文详情
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("Details/Home/Posts/{PostPutSortTime}/{PostId}")]
        [HttpGet]
        public IActionResult Posts(PostBindModel model)
        {
            if (null == model || !model.PostId.HasValue || model.PostPutSortTime.IsNullOrWhitespace())
                return RedirectToAction("Index");

            // 
            var viewModel = this._viewProjectionFactory.GetViewProjection<PostBindModel, PostViewModel>(model);

            Set();
            return View("PostDetails", viewModel);
        }

        #endregion



        #region 根据标签检索博文

        /// <summary>
        /// 根据标签搜索博文
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Route("Tagposts/Home/Tags/{tag}/{page}")]
        [HttpGet]
        public IActionResult Tags(string tag, string page)
        {
            if (tag.IsNullOrWhitespace() || page.IsNullOrWhitespace())
                return View("Index", null);

            int pageNum = 1;
            int.TryParse(page, out pageNum);

            var viewModel = this._viewProjectionFactory.GetViewProjection<TagPostsBindModel, TagPostsViewModel>(new TagPostsBindModel() { TagName = tag, PageNum = pageNum });

            Set();
            return View("TagPosts", viewModel);
        }


        #endregion



        #region 查询并设置标签和最近博文

        private void Set()
        {
            ViewBag.Tags = GetTags();
            ViewBag.RecentPost = GetRecentPostAction();
            ViewBag.Configs = this._appConfig.Value;
        }

        /// <summary>
        /// 设置展示的标签 到ViewBag.Tags
        /// </summary>
        private TagsViewModel GetTags()
        {
            TagsViewModel viewModel = null;
            if (!this._memoryCache.TryGetValue(MemoryCacheKeys.Tags, out viewModel))
            {
                viewModel = this._viewProjectionFactory.GetViewProjection<TagsBindModel, TagsViewModel>(new TagsBindModel());
                this._memoryCache.Set(MemoryCacheKeys.Tags, viewModel);
            }
            return viewModel;
        }

        /// <summary>
        /// 设置展示的最近文章 到ViewBag.RecentPost
        /// </summary>
        private RecentPostViewModel GetRecentPostAction()
        {
            RecentPostViewModel viewModel = null;
            if (!this._memoryCache.TryGetValue(MemoryCacheKeys.RecentPost, out viewModel))
            {
                viewModel = this._viewProjectionFactory.GetViewProjection<RecentPostBindModel, RecentPostViewModel>(new RecentPostBindModel());
                this._memoryCache.Set(MemoryCacheKeys.RecentPost, viewModel);
            }
            return viewModel;
        }

        #endregion
    }
}
