<template>
    <el-col :sm="24" :md="pzoption.mdwidth">
        <i class="iconfont icon-shezhi pull-right widgetset hidden-print" @click.stop="dialogInputVisible = true"></i>
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <el-form-item :label="pzoption.title" :prop="'wigetitems.' + index + '.value'" :rules="childpro.rules">
            <el-input :placeholder="childpro.placeholder"  :readonly="childpro.readonly" v-model="pzoption.value" clearable>
            </el-input>
        </el-form-item>
        <el-dialog title="组件属性" :visible.sync="dialogInputVisible">
            <el-form :model="childpro">
                <el-form-item label="序列号格式">
                    <el-radio v-model="childpro.itemtype" label="0">YYYYMMDD000</el-radio>
                    <!--<el-radio v-model="childpro.itemtype" label="1">多行文本</el-radio>-->
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
                pdid: ComFunJS.getQueryString('pdid', '0'),
                childpro: {
                    placeholder: "占位符",
                    readonly: true,
                    itemtype: "0",
                    rules: {
                        required: false, message: '不能为空', type: "string"
                    }
                }
            }
        },
        methods: {
            delWid: function (wigdetcode) {
                // 子组件中触发父组件方法ee并传值cc12345
                this.$root.nowwidget = {};
                _.remove(this.$root.FormData.wigetitems, function (obj) {
                    return obj.wigdetcode == wigdetcode;
                });
            },
            senddata: function () {
                this.$emit('data-change', JSON.stringify(this.childpro));
            }
        },
        mounted: function () {
            var pro = this;
            pro.$nextTick(function () {
                if (pro.pdid!="0") {
                    $.getJSON("/API/VIEWAPI.ashx", { Action: "FORMBI_CRWFNUM", P1: pro.pdid }, function (result) {
                        if (!result.ErrorMsg) {
                            pro.pzoption.value = result.Result.split('-')[1];
                            if (pro.pzoption.childpro.placeholder) {
                                pro.childpro = pro.pzoption.childpro
                            }
                        }
                    })

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