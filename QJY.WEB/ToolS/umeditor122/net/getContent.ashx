<%@ WebHandler  Language="C#"  Class="getContent" %>
/**
 * Created by visual studio 2010
 * User: xuheng
 * Date: 12-3-6
 * Time: 下午21:23
 * To get the value of editor and output the value .
 */
using System;
using System.Web;

public class getContent : IHttpHandler {
    
    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/html";

        //获取数据
        string content = context.Request.Form["myEditor"];

        
        //存入数据库或者其他操�?        //-------------

        //显示


        context.Response.Write("<script src='../third-party/jquery.min.js'></script>");
        context.Response.Write("<script src='../third-party/mathquill/mathquill.min.js'></script>");
        context.Response.Write("<link rel='stylesheet' href='../third-party/mathquill/mathquill.css'/>");
        context.Response.Write("<div class='content'>" + content + "</div>");

    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}