<template>
    <el-col :sm="24" :md="pzoption.mdwidth">
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <div class="Ui_Ct_Line padding10">
            <div v-text="pzoption.title"></div>
        </div>
    </el-col>

</template>
<style>
    .Ui_Ct_Line {
        -ms-flex-align: center !important;
        align-items: center !important;
        display: -ms-flexbox;
        display: flex;
        -ms-flex-wrap: wrap;
        flex-wrap: wrap;
    }

        .Ui_Ct_Line > * {
            -ms-flex: 0 0 auto;
            flex: 0 0 auto;
            width: auto;
            max-width: none;
            display: block;
            min-height: 1px;
            padding: 0 20px;
        }

        .Ui_Ct_Line::before, .Ui_Ct_Line::after {
            content: ' ';
            -ms-flex-preferred-size: 0;
            flex-basis: 0;
            -ms-flex-positive: 1;
            flex-grow: 1;
            display: block;
            position: relative;
            width: 100%;
            min-height: 1px;
            background-color: #CBCBCB;
        }
</style>
<script>
    module.exports = {
        props: ['pzoption', 'index'],
        methods: {
            delWid: function (wigdetcode) {
                // 子组件中触发父组件方法ee并传值cc12345
                this.$root.nowwidget = { };
                _.remove(this.$root.FormData.wigetitems, function (obj) {
                    return obj.wigdetcode == wigdetcode;
                });
            }
        }
    };
</script>