<template>
    <el-col :sm="24" :md="pzoption.mdwidth">
        <i class="iconfont icon-shezhi pull-right widgetset hidden-print" @click.stop="dialogInputVisible = true"></i>
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <el-form-item :label="pzoption.title">
            <el-button type="primary" icon="el-icon-search" @click="queryYB" style="vertical-align:bottom;">{{ childpro.placeholder }}</el-button>
        </el-form-item>
        <el-dialog title="组件属性" :visible.sync="dialogInputVisible">
            <el-form :model="childpro">
                <el-form-item label="占位符">
                    <el-input v-model="childpro.placeholder" autocomplete="off"></el-input>
                </el-form-item>
            </el-form>
        </el-dialog>
    </el-col>

</template>
<script>
    module.exports = {
        props: ['pzoption', 'index'],
        data: function () {
            return {
                dialogInputVisible: false,
                childpro: {
                    placeholder: "搜索按钮"
                }
            }
        },
        methods: {
            delWid: function (wigdetcode) {
                this.$root.nowwidget = {};//没这个删除不掉啊
                _.remove(this.$root.FormData.wigetitems, function (obj) {
                    return obj.wigdetcode == wigdetcode;
                });
            },
            senddata: function () {
                this.$emit('data-change', JSON.stringify(this.childpro));
            },
            queryYB: function () {
                this.$root.GetYBData()
            }
        },
        mounted: function () {
            var chi = this;
            chi.$nextTick(function () {
                if (chi.$root.addchildwig) {
                    chi.$root.addchildwig();//不能缺少
                }
                if (chi.pzoption.childpro.placeholder) {
                    chi.childpro = chi.pzoption.childpro
                }
            })

        },
        watch: {
            childpro: { //深度监听，可监听到对象、数组的变化
                handler(newV, oldV) {
                    this.senddata();
                },
                deep: true
            }
        }
    };
</script>