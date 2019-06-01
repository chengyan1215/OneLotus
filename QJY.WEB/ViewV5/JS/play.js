    var onResizeWindow = function(mode){
		
		//获取浏览器的可视宽度
		var viewWidth = $(window).width();
		//获取浏览器的可视高度
		var viewHeight = $(window).height();
		
		if($('.left_side').hasClass('fixed')){
			aSideHeight = $(window).height()-98;//左边栏浮动时的高度
		}else{
			aSideHeight = $(window).height()-118;//左边栏浮动时的高度
		}

		if($('.right_side').hasClass('fixed')){
			nSideHeight = $(window).height()-223;//右边栏浮动时的高度
// 			dySideHeight = $(window).height()-223;//右边栏浮动时的高度
			faqSideHeight = $(window).height()-241;//右边栏浮动时的高度
		}else{
			nSideHeight = $(window).height()-252;//右边栏浮动时的高度
// 			dySideHeight = $(window).height()-252;//右边栏浮动时的高度
			faqSideHeight = $(window).height()-270;//右边栏浮动时的高度
		}
		
		$('.main_content').css('min-height',viewHeight-74);
		$('.l_zy_list').css('height',aSideHeight);
		$('#chapList').css('height', '105px');
		$('.l_note_list').css('height',nSideHeight);
		$('.r_faq_list').css('height',faqSideHeight);
 		$('#mediaspace_wrapper').css('height',viewHeight-149);
 		$('#textcontent').css('height',viewHeight-240);

 		$('#scormcontent').css('height',viewHeight-149);
 		$('#scormcontent').css('width',$('.video_box').width());
 		
 		$('#audiocontent').css('top',viewHeight-185);
 		$('#audiocontent .audiojs .scrubber').css('width',$('.video_box').width()-160);

 		$('#mediaspace_wrapper').css('width',$('.video_box').width());
		$('.video_wrap').css('height',viewHeight-149);
		$('.video_box').css('height', viewHeight - 149);	/* 7像素问题待查看 */
		$('.vppt').css('height', viewHeight - 149);	/* 7像素问题待查看 */
		$(".iframeppt").css('height', viewHeight - 149)

	};
	var resizePlayer = function() {
	    var width = $('.video_box').width();
	    var height = $('.video_box').height();
	    //jwplayer().resize(width, height);
	};
	window.onresize = function(){
		onResizeWindow();
	}
	$(function() {

		onResizeWindow();

		
		$('.tag_intro').mouseover(function() {
	        $('#intro_detail').show().attr('disp', '1');
		})
        .mouseout(function () {
	        $('#intro_detail').attr('disp', '0');
	        moDetail = setTimeout("hideIntroDetail()", 300);
	    });
	});

	//tab切换 
	//答疑
    function show_answer(qid) {
		var answer = '#show_answer_' + qid;
		var sa_tip = '#sa_tip_' + qid;
		var ca_tip = '#ca_tip_' + qid;
		$(sa_tip).hide();
		$(ca_tip).show();
		$(answer).show();
    }
    function close_answer(qid) {
    	var answer = '#show_answer_' + qid;
    	var sa_tip = '#sa_tip_' + qid;
		var ca_tip = '#ca_tip_' + qid;
		$(ca_tip).hide();
		$(sa_tip).show();
    	$(answer).hide();
    }
    $('#my_question').val('');
    $('.my_question_text').focus(function(){
    	$('#my_question').focus();
    });
    $('#my_question').keyup(function(){
    	$('#my_question_submit').removeClass('op');
    	question_tips('可输入3-100字', 0);
    });
    $('#my_question').focus(function() {
    	$('.my_question_text').hide();
    	$('.my_question_input').addClass('state-focus');
    });
    $('#my_question').blur(function(){
    	if($('#my_question').val() == '') {
    		$('.my_question_input').removeClass('state-focus');
    		$('.my_question_text').show();
    	}
    });
    $('#my_question_submit').click(function(){
    	if ($(this).hasClass("op")) {
            return false;
        };
        $(this).addClass("op");
        var my_question = $('#my_question').val();
    	var reg = /\n/g;
    	var my_question_size = my_question.replace(reg, "").length;
//    	var my_question_size = dystrlen(my_question.replace(reg, ""));
    	if(my_question == '' || my_question_size < 3 || my_question_size > 100) {
    		question_tips('最少输入3个字', 1);
			return;
		}
    	$.ajax({
            url: "",
            type: "post",
            dataType: "json",
            data: "sid=" + sid + "&cid=" + curcid + "&courseid=" + courseid + "&q=" + my_question,
            success: function(ret) {
                if (ret.code == 0) {
                	$('.dy-list').prepend(ret.data);
                	$('#my_question').val('');
               	 	$('#my_question').blur();
	               	var st = r_raq_list.getScrollTop();
	             	r_raq_list.initSlide();
	             	r_raq_list.scrollTop(st);
//               	 	new okScroll($('.r_faq_list'),{scrollTo:0});
                } else {
                	question_tips(ret.data, 1);
                }
            }
        });
    });