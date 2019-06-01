<template>
    <el-col :sm="24" :md="pzoption.mdwidth">
        <i class="iconfont icon-shezhi pull-right widgetset hidden-print" @click.stop="dialogInputVisible = true"></i>
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <el-form-item :label="pzoption.title" :prop="'wigetitems.' + index + '.value'" :rules="childpro.rules">
            <el-input v-model="pzoption.value" style="display:none">
            </el-input>
            <el-upload class="upload-demo"
                       :action="action"
                       :on-preview="handlePreview"
                       :on-remove="handleRemove"
                        multiple
                       :limit="childpro.limit"
                       :on-error="handleError"
                       :on-success="handleSuncess"
                       :on-exceed="handleExceed"
                       :file-list="childpro.fileList"
                       :disabled="childpro.disabled">
                <el-button size="small" type="primary" class="hidden-print" :disabled="childpro.disabled">点击上传附件</el-button>
                <div slot="tip" class="el-upload__tip hidden-print" v-text="childpro.placeholder"></div>
            </el-upload>
        </el-form-item>
        <el-dialog title="组件属性" :visible.sync="dialogInputVisible">
            <el-form :model="childpro">

                <el-form-item label="占位符">
                    <el-input v-model="childpro.placeholder" autocomplete="off"></el-input>
                </el-form-item>
                <el-form-item label="必填">
                    <el-switch v-model="childpro.rules.required"></el-switch>
                </el-form-item>
                <el-form-item label="只读">
                    <el-switch v-model="childpro.disabled"></el-switch>
                </el-form-item>
                <el-form-item label="上传数量限制">
                    <el-input-number v-model="childpro.limit" :min="1" :max="10" label="上传数量限制"></el-input-number>
                </el-form-item>
                <el-form-item label="上传文件类型限制">
                    <el-input v-model="childpro.accept"></el-input>
                </el-form-item>

            </el-form>
        </el-dialog>
    </el-col>

</template>
<script>
    module.exports = {
        props: ['pzoption', 'index'],
        data() {
            return {
                action: ComFunJS.getCookie("fileapi") + "/document/fileupload/" + ComFunJS.getCookie("qycode"),
                dialogInputVisible: false,
                childpro: {
                    placeholder: "",
                    fileList: [],
                    limit :3,
                    accept :".doc,.docx,.xls,.xlsx,.ppt,.pptx,.pdf,.jpeg,.png,.zip,.gif,.jpg",
                    disabled: false,
                    rules: {
                        required: false, message: '文件不能为空', type: "string"
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
            },
            handleError: function (err, file, fileList) {
                alert(this.action)
                //console.log(err, file, fileList);
            },
            handleSuncess: function (response, file, fileList) {
                this.childpro.fileList = fileList;
                this.pzoption.value = JSON.stringify(fileList);
                // 检查是否是图片
                //var filePath = $(this).val(),
                //    fileFormat = filePath.substring(filePath.lastIndexOf(".")).toLowerCase();
                //
                //if( !fileFormat.match(/.png|.jpg|.jpeg/) ) {
                //    showError('文件格式必须为：png/jpg/jpeg');
                //    return;
                //}
            },
            handlePreview(file) {
                    var type= _.last(file.name.split("."));
                    if ($.inArray(type.toLowerCase(), ['doc', 'docx', 'ppt', 'pptx', 'pdf']) > -1) {
                          var url = ComFunJS.getCookie("fileapi") + ComFunJS.getCookie("qycode") + /document/ + file.response.split(',')[0];
                          location.href = url;
                    }
                    else
                    {
                          var url = ComFunJS.getCookie("fileapi") + ComFunJS.getCookie("qycode") + /document/ + file.response.split(',')[0];
                          location.href = url;
                    }
            },
            handleRemove(file, fileList) {
                this.childpro.fileList = fileList;
                this.pzoption.value = JSON.stringify(fileList);
            },
            handleExceed(files, fileList) {
                this.$message.warning('超出上传数量为' + this.childpro.limit + '的限制');
            },
        },
         mounted: function () {
            var pro = this;
            pro.$nextTick(function () {
                if (this.pzoption.childpro.limit) {
                    this.childpro = this.pzoption.childpro
                } else {
                    //首次的时候赋予默认值
                    this.pzoption.childpro = this.childpro;
                }
            })
         },
         watch: {
            childpro: { //深度监听，可监听到对象、数组的变化
                handler(newV, oldV) {
                    this.senddata();
                },
                deep: true
            },
           "pzoption.value": { //深度监听，可监听到对象、数组的变化
                handler(newV, oldV) {
                    this.childpro = this.pzoption.childpro;
                },
                deep: true
            }
           }

    };
</script>