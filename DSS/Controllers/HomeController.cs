﻿using DSS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DSS.Data;
using DSS.Repository;
using System;
using DSS.Repository.CommentRepository;
using DSS.Service.HomeService;


namespace DSS.Controllers;

public class HomeController : Controller
{
    
    private readonly INewsRepository _newsRepository;
    private readonly IHomeService _homeService;
    private readonly ICommentRepository _commentRepository;

    public HomeController( INewsRepository newsRepository,IHomeService homeService,ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
        _homeService = homeService;
        _newsRepository = newsRepository;
    }

    public async Task<IActionResult> Index(int id)
    {
        

        var sessionFlag = HttpContext.Session.GetString("username") == null;
        ViewBag.SessionFlag = sessionFlag;

        var takeStart = id * 10 - 10;
        if (takeStart == 0) takeStart = 0;
        IEnumerable<NewsModel?> newsList = await _newsRepository.getAllNewsAsync();
        newsList = newsList.Reverse();
        List< NewsCommentCountDTO > newsAndCommentsList = new List< NewsCommentCountDTO >();
        
        foreach (var news in newsList)
        {
          IEnumerable<CommentModel>? Commnets =  await _commentRepository.getCommentsByNewsIdAsync(news.Id);
          int count = (Commnets != null) ? Commnets.Count() : 0;
          NewsCommentCountDTO newsWithCommentsCount = new NewsCommentCountDTO();
          newsWithCommentsCount.commentCount = count;
          newsWithCommentsCount.news = news;
          newsAndCommentsList.Add(newsWithCommentsCount);
        }
       

        



        var dataTransfer = new NewsDTO();
        dataTransfer.latestNewsList = newsAndCommentsList.Take(4).ToList();
        dataTransfer.mainNewsList = await _homeService.getMainNewsListWithCountAsync(newsAndCommentsList);
        dataTransfer.allNewsList = newsAndCommentsList.Skip(4).Skip(takeStart).Take(10).ToList();
        dataTransfer.newsCount = newsAndCommentsList.Skip(4).Count();

        var pageCount = Math.Ceiling(dataTransfer.newsCount / 10);




        if (id <= 0)
            return RedirectToAction("Index", "Home", new { id = 1 });

        if (id > pageCount && id != 1)
            return RedirectToAction("Index", "Home", new { id = pageCount });


        


        return View(dataTransfer);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}