// JavaScript Document
//定义Beautifier的构造函数
var DialogFx = function(ele, opt) {
    this.$element = ele,
    this.defaults = {
		'id': 'somedialog',
		'dialogFx': '0',
    },
    this.options = $.extend({}, this.defaults, opt)
}
//定义DialogFx的显示方法
DialogFx.prototype = {
    Show: function() {
		$("#"+this.options.id).removeClass("dialog--close");
		$("#"+this.options.id).addClass("dialog--open");	
    },
	Hidden: function() {
		$("#"+this.options.id).removeClass("dialog--open");
		$("#"+this.options.id).addClass("dialog--close");
    }
}
//在插件中使用DialogShow对象
$.fn.DialogShow = function(options) {
    //创建Beautifier的实体
    var dialogFx = new DialogFx(this, options);
	if(options.dialogFx == "0"){
		return dialogFx.Hidden();
	} else {
		//调用其方法
    	return dialogFx.Show();			
	}
}
