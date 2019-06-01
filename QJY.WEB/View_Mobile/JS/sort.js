//自由驱动工作室
//作者：林鑫
$.fn.scrollTo = function (options) {
    var defaults = {
        toT: options,    //滚动目标位置
        durTime: 500,  //过渡动画时间
        delay: 30,     //定时器时间
        callback: null   //回调函数
    };
    var opts = $.extend(defaults, options),
        timer = null,
        _this = this,
        curTop = _this.scrollTop(),//滚动条当前的位置
        subTop = opts.toT - curTop,    //滚动条目标位置和当前位置的差值
        index = 0,
        dur = Math.round(opts.durTime / opts.delay),
        smoothScroll = function (t) {
            index++;
            var per = Math.round(subTop / dur);
            if (index >= dur) {
                _this.scrollTop(t);
                window.clearInterval(timer);
                if (opts.callback && typeof opts.callback == 'function') {
                    opts.callback();
                }
                return;
            } else {
                _this.scrollTop(curTop + index * per);
            }
        };
    timer = window.setInterval(function () {
        smoothScroll(opts.toT);
    }, opts.delay);
    return _this;
};

function initsort() {
    var Initials = $('.initials');
    var LetterBox = $('#letter');
    Initials.find('ul').append('<li>A</li><li>B</li><li>C</li><li>D</li><li>E</li><li>F</li><li>G</li><li>H</li><li>I</li><li>J</li><li>K</li><li>L</li><li>M</li><li>N</li><li>O</li><li>P</li><li>Q</li><li>R</li><li>S</li><li>T</li><li>U</li><li>V</li><li>W</li><li>X</li><li>Y</li><li>Z</li><li>#</li>');
    initialsort();

    $(".initials ul li").click(function () {
        var _this = $(this);
        var LetterHtml = _this.html();
        //LetterBox.html(LetterHtml).fadeIn();
        LetterBox.html(LetterHtml).show();

        //Initials.css('background', 'rgba(145,145,145,0.6)');

        setTimeout(function () {
            //Initials.css('background', 'rgba(145,145,145,0)');
            //LetterBox.fadeOut();
            LetterBox.hide();
        }, 1000);

        var _index = _this.index()
        if (_index == 0) {
            //$('html,body').animate({ scrollTop: '0px' }, 300);//点击第一个滚到顶部
            $('.content').scrollTo(0);
        } else if (_index == 26) {
            var DefaultTop = $('#default').position().top;
            //$('html,body').animate({ scrollTop: DefaultTop + 'px' }, 300);//点击最后一个滚到#号
            $('.content').scrollTo(DefaultTop);
        } else {
            var letter = _this.text();
            if ($('#' + letter).length > 0) {
                var LetterTop = $('#' + letter).position().top;
                //$('html,body').animate({ scrollTop: LetterTop - 45 + 'px' }, 300);
                var lt = 0;
                //if (modeltxl.zuijin.size() > 0) { lt = 110;}
                $('.content').scrollTo(LetterTop + lt + 50);
            }
        }
    })

    var windowHeight = $(window).height();
    var InitHeight = windowHeight - 45 - $(".bar-tab").height();
    Initials.height(InitHeight);
    var LiHeight = InitHeight / 28;
    Initials.find('li').height(LiHeight);
}

var initials = [];

function initialsort() {//公众号排序
    var SortList = $(".sort_list");
    var SortBox = $(".sort_box");
    SortList.sort(asc_sort).appendTo('.sort_box');//按首字母排序
    function asc_sort(a, b) {
        return makePy($(b).find('.num_name').text().charAt(0))[0].toUpperCase() < makePy($(a).find('.num_name').text().charAt(0))[0].toUpperCase() ? 1 : -1;
    }

    var num = 0;
    SortList.each(function (i) {
        var initial = makePy($(this).find('.num_name').text().charAt(0))[0].toUpperCase();
        if (initial >= 'A' && initial <= 'Z') {
            if (initials.indexOf(initial) === -1)
                initials.push(initial);
        } else {
            num++;
        }

    });

    $.each(initials, function (index, value) {//添加首字母标签
        //SortBox.append('<div class="sort_letter" id="' + value + '">' + value + '</div>');
        SortBox.append('<li class="list-group-title" id="' + value + '">' + value + '</li>');
    });
    if (num != 0) {
        //SortBox.append('<div class="sort_letter" id="default">#</div>');
        SortBox.append('<li class="list-group-title" id="default">#</li>');
    }

    for (var i = 0; i < SortList.length; i++) {//插入到对应的首字母后面
        var letter = makePy(SortList.eq(i).find('.num_name').text().charAt(0))[0].toUpperCase();
        switch (letter) {
            case "A":
                $('#A').after(SortList.eq(i));
                break;
            case "B":
                $('#B').after(SortList.eq(i));
                break;
            case "C":
                $('#C').after(SortList.eq(i));
                break;
            case "D":
                $('#D').after(SortList.eq(i));
                break;
            case "E":
                $('#E').after(SortList.eq(i));
                break;
            case "F":
                $('#F').after(SortList.eq(i));
                break;
            case "G":
                $('#G').after(SortList.eq(i));
                break;
            case "H":
                $('#H').after(SortList.eq(i));
                break;
            case "I":
                $('#I').after(SortList.eq(i));
                break;
            case "J":
                $('#J').after(SortList.eq(i));
                break;
            case "K":
                $('#K').after(SortList.eq(i));
                break;
            case "L":
                $('#L').after(SortList.eq(i));
                break;
            case "M":
                $('#M').after(SortList.eq(i));
                break;
            case "N":
                $('#N').after(SortList.eq(i));
                break;
            case "O":
                $('#O').after(SortList.eq(i));
                break;
            case "P":
                $('#P').after(SortList.eq(i));
                break;
            case "Q":
                $('#Q').after(SortList.eq(i));
                break;
            case "R":
                $('#R').after(SortList.eq(i));
                break;
            case "S":
                $('#S').after(SortList.eq(i));
                break;
            case "T":
                $('#T').after(SortList.eq(i));
                break;
            case "U":
                $('#U').after(SortList.eq(i));
                break;
            case "V":
                $('#V').after(SortList.eq(i));
                break;
            case "W":
                $('#W').after(SortList.eq(i));
                break;
            case "X":
                $('#X').after(SortList.eq(i));
                break;
            case "Y":
                $('#Y').after(SortList.eq(i));
                break;
            case "Z":
                $('#Z').after(SortList.eq(i));
                break;
            default:
                $('#default').after(SortList.eq(i));
                break;
        }
    };
}