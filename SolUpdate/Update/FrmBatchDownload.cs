﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.Collections;
using System.Threading;
using System.Diagnostics;

namespace Update
{
    public partial class FrmBatchDownload : Form
    {
        #region 全局成员

        //存放下载列表
        List<SynFileInfo> m_SynFileInfoList;
        UpdateService.WebService service = null;//webservice服务
        WebClient wc = null;
        string url;//获取下载地址 
        string[] zips;//获取升级压缩包
        #endregion

        #region 构造函数

        public FrmBatchDownload()
        {
            InitializeComponent();
            m_SynFileInfoList = new List<SynFileInfo>();
        }

        #endregion

        #region 窗体加载事件

        private void FrmBatchDownload_Load(object sender, EventArgs e)
        {
            //初始化DataGridView相关属性
            InitDataGridView(dgvDownLoad);
            //添加DataGridView相关列信息
            AddGridViewColumns(dgvDownLoad);
            //新建任务
            AddBatchDownload();
        }

        #endregion

        #region 添加GridView列

        /// <summary>
        /// 正在同步列表
        /// </summary>
        void AddGridViewColumns(DataGridView dgv)
        {
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "DocID",
                HeaderText = "文件ID",
                Visible = false,
                Name = "DocID"
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                DataPropertyName = "DocName",
                HeaderText = "文件名",
                Name = "DocName",
                Width = 200
            });
            //dgv.Columns.Add(new DataGridViewTextBoxColumn()
            //{
            //    DataPropertyName = "FileSize",
            //    HeaderText = "大小",
            //    Name = "FileSize",
            //});
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "SynSpeed",
                HeaderText = "速度",
                Name = "SynSpeed"
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "SynProgress",
                HeaderText = "进度",
                Name = "SynProgress"
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "DownPath",
                HeaderText = "下载地址",
                Visible = false,
                Name = "DownPath"
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "SavePath",
                HeaderText = "保存地址",
                Visible = false,
                Name = "SavePath"
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Async",
                HeaderText = "是否异步",
                Visible = false,
                Name = "Async"
            });
        }

        #endregion

        #region 添加下载任务并显示到列表中

        void AddBatchDownload()
        {
            try
            {
                service = new UpdateService.WebService();//webservice服务
                url = service.GetUrl();//获取下载地址                
                zips = service.GetZips();//获取升级压缩包
                //清空行数据
                dgvDownLoad.Rows.Clear();
                //添加列表(建立多个任务)
                List<ArrayList> arrayListList = new List<ArrayList>();
                for (int i = 0; i < zips.Length; i++)
                {
                    string fileurl = url + zips[i];
                    string saveurl = Application.StartupPath +"\\"+ zips[i];
                    arrayListList.Add(new ArrayList(){
                i.ToString (),//文件id
               zips [i],//文件名称
                "21.2 MB",//文件大小
                "0 KB/S",//下载速度
                "0%",//下载进度
                //  "http://210.83.203.60:89/fileupdate/WFormSrc.exe",
                fileurl,//远程服务器下载地址
                saveurl,//本地保存地址
                true//是否异步
            });

                }
               

                foreach (ArrayList arrayList in arrayListList)
                {
                    int rowIndex = dgvDownLoad.Rows.Add(arrayList.ToArray());
                    arrayList[2] = 0;
                    arrayList.Add(dgvDownLoad.Rows[rowIndex]);
                    //取出列表中的行信息保存列表集合(m_SynFileInfoList)中
                    m_SynFileInfoList.Add(new SynFileInfo(arrayList.ToArray()));
                }
            }
            catch(Exception ex)
            { 
            
            }
        }

        #endregion

        #region 开始下载按钮单机事件

        private void btnStartDownLoad_Click(object sender, EventArgs e)
        {
            //判断网络连接是否正常
            if (isConnected())
            {
                //设置不可用
                btnStartDownLoad.Enabled = false;
                //设置最大活动线程数以及可等待线程数
                ThreadPool.SetMaxThreads(3, 3);
                //判断是否还存在任务
                if (m_SynFileInfoList.Count <= 0) AddBatchDownload();
                foreach (SynFileInfo m_SynFileInfo in m_SynFileInfoList)
                {
                    //启动下载任务
                    StartDownLoad(m_SynFileInfo);
                }
                string serviceVersion = service.getver ();
                Db.path = AppDomain.CurrentDomain.BaseDirectory + "Update.ini";
                Db.IniWriteValue("update", "version", serviceVersion);
                MessageBox.Show("升级成功！");
                Application.Exit();//退出升级程序
                Process.Start("WFormSrc.exe");//打开主程序Main.exe
            }
            else
            {
                MessageBox.Show("网络异常!");
            }
        }

        #endregion

        #region 检查网络状态

        //检测网络状态
        [DllImport("wininet.dll")]
        extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);
        /// <summary>
        /// 检测网络状态
        /// </summary>
        bool isConnected()
        {
            int I = 0;
            bool state = InternetGetConnectedState(out I, 0);
            return state;
        }

        #endregion

        #region 使用WebClient下载文件

        /// <summary>
        /// HTTP下载远程文件并保存本地的函数
        /// </summary>
        void StartDownLoad(object o)
        {
            SynFileInfo m_SynFileInfo = (SynFileInfo)o;
            m_SynFileInfo.LastTime = DateTime.Now;
            //再次new 避免WebClient不能I/O并发 
            WebClient client = new WebClient();
            if (m_SynFileInfo.Async)
            {
                //异步下载
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri(m_SynFileInfo.DownPath), m_SynFileInfo.SavePath, m_SynFileInfo);
            }
            else client.DownloadFile(new Uri(m_SynFileInfo.DownPath), m_SynFileInfo.SavePath);
        }

        /// <summary>
        /// 下载进度条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            SynFileInfo m_SynFileInfo = (SynFileInfo)e.UserState;
            m_SynFileInfo.SynProgress = e.ProgressPercentage + "%";
            double secondCount = (DateTime.Now - m_SynFileInfo.LastTime).TotalSeconds;
            m_SynFileInfo.SynSpeed = FileOperate.GetAutoSizeString(Convert.ToDouble(e.BytesReceived / secondCount), 2) + "/s";
            //更新DataGridView中相应数据显示下载进度
            m_SynFileInfo.RowObject.Cells["SynProgress"].Value = m_SynFileInfo.SynProgress;
            //更新DataGridView中相应数据显示下载速度(总进度的平均速度)
            m_SynFileInfo.RowObject.Cells["SynSpeed"].Value = m_SynFileInfo.SynSpeed;
        }

        /// <summary>
        /// 下载完成调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //到此则一个文件下载完毕
            SynFileInfo m_SynFileInfo = (SynFileInfo)e.UserState;
            m_SynFileInfoList.Remove(m_SynFileInfo);
            if (m_SynFileInfoList.Count <= 0)
            {
                //此时所有文件下载完毕
                btnStartDownLoad.Enabled = true;
            }
        }

        #endregion

        #region 需要下载文件实体类

        class SynFileInfo
        {
            public string DocID { get; set; }
            public string DocName { get; set; }
            public long FileSize { get; set; }
            public string SynSpeed { get; set; }
            public string SynProgress { get; set; }
            public string DownPath { get; set; }
            public string SavePath { get; set; }
            public DataGridViewRow RowObject { get; set; }
            public bool Async { get; set; }
            public DateTime LastTime { get; set; }

            public SynFileInfo(object[] objectArr)
            {
                int i = 0;
                DocID = objectArr[i].ToString(); i++;
                DocName = objectArr[i].ToString(); i++;
                FileSize = Convert.ToInt64(objectArr[i]); i++;
                SynSpeed = objectArr[i].ToString(); i++;
                SynProgress = objectArr[i].ToString(); i++;
                DownPath = objectArr[i].ToString(); i++;
                SavePath = objectArr[i].ToString(); i++;
                Async = Convert.ToBoolean(objectArr[i]); i++;
                RowObject = (DataGridViewRow)objectArr[i];
            }
        }

        #endregion

        #region 初始化GridView

        void InitDataGridView(DataGridView dgv)
        {
            dgv.AutoGenerateColumns = false;//是否自动创建列
            dgv.AllowUserToAddRows = false;//是否允许添加行(默认：true)
            dgv.AllowUserToDeleteRows = false;//是否允许删除行(默认：true)
            dgv.AllowUserToResizeColumns = false;//是否允许调整大小(默认：true)
            dgv.AllowUserToResizeRows = false;//是否允许调整行大小(默认：true)
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;//列宽模式(当前填充)(默认：DataGridViewAutoSizeColumnsMode.None)
            dgv.BackgroundColor = System.Drawing.Color.White;//背景色(默认：ControlDark)
            dgv.BorderStyle = BorderStyle.Fixed3D;//边框样式(默认：BorderStyle.FixedSingle)
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;//单元格边框样式(默认：DataGridViewCellBorderStyle.Single)
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;//列表头样式(默认：DataGridViewHeaderBorderStyle.Single)
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;//是否允许调整列大小(默认：DataGridViewColumnHeadersHeightSizeMode.EnableResizing)
            dgv.ColumnHeadersHeight = 30;//列表头高度(默认：20)
            dgv.MultiSelect = false;//是否支持多选(默认：true)
            dgv.ReadOnly = true;//是否只读(默认：false)
            dgv.RowHeadersVisible = false;//行头是否显示(默认：true)
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;//选择模式(默认：DataGridViewSelectionMode.CellSelect)
        }

        #endregion

        #region 文件相关操作类分

        /// <summary>
        /// 文件有关的操作类
        /// </summary>
        public class FileOperate
        {
            #region 相应单位转换常量

            private const double KBCount = 1024;
            private const double MBCount = KBCount * 1024;
            private const double GBCount = MBCount * 1024;
            private const double TBCount = GBCount * 1024;

            #endregion

            #region 获取适应大小

            /// <summary>
            /// 得到适应大小
            /// </summary>
            /// <param name="size">字节大小</param>
            /// <param name="roundCount">保留小数(位)</param>
            /// <returns></returns>
            public static string GetAutoSizeString(double size, int roundCount)
            {
                if (KBCount > size) return Math.Round(size, roundCount) + "B";
                else if (MBCount > size) return Math.Round(size / KBCount, roundCount) + "KB";
                else if (GBCount > size) return Math.Round(size / MBCount, roundCount) + "MB";
                else if (TBCount > size) return Math.Round(size / GBCount, roundCount) + "GB";
                else return Math.Round(size / TBCount, roundCount) + "TB";
            }

            #endregion
        }

        #endregion




    }
}
