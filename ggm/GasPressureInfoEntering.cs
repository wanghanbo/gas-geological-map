﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using GIS;
using GIS.Common;
using LibCommon;
using LibEntity;

namespace ggm
{
    public partial class GasPressureInfoEntering : Form
    {
        /// <summary>
        ///     构造方法
        /// </summary>
        public GasPressureInfoEntering()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     带参数的构造方法
        /// </summary>
        public GasPressureInfoEntering(GasPressure gasPressure)
        {
            GasPressure = gasPressure;
            InitializeComponent();
        }

        private GasPressure GasPressure { get; set; }

        /// <summary>
        ///     20140308 lyf 加载窗体时传入拾取点的坐标值
        /// </summary>
        /// <param name="sender"></params>
        /// <params name="e"></params>
        private void GasPressureInfoEntering_Load(object sender, EventArgs e)
        {
            dtpMeasureDateTime.Format = DateTimePickerFormat.Custom;
            dtpMeasureDateTime.CustomFormat = @"yyyy/MM/dd HH:mm:ss";
            // 坐标X
            if (GasPressure == null) return;
            txtCoordinateX.Text = GasPressure.coordinate_x.ToString(CultureInfo.InvariantCulture);
            txtCoordinateY.Text = GasPressure.coordinate_y.ToString(CultureInfo.InvariantCulture);
            txtCoordinateZ.Text = GasPressure.coordinate_z.ToString(CultureInfo.InvariantCulture);
            txtDepth.Text = GasPressure.depth.ToString(CultureInfo.InvariantCulture);
            txtGasPressureValue.Text = GasPressure.gas_pressure_value.ToString(CultureInfo.InvariantCulture);
            dtpMeasureDateTime.Value = GasPressure.measure_date_time;
            selectTunnelSimple1.SetTunnel(GasPressure.tunnel);
        }

        /// <summary>
        ///     提  交
        /// </summary>
        /// <params name="sender"></params>
        /// <params name="e"></params>
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            // 创建一个瓦斯含量点实体
            if (GasPressure == null)
            {
                var gasPressure = new GasPressure
                {
                    coordinate_x = Convert.ToDouble(txtCoordinateX.Text),
                    coordinate_y = Convert.ToDouble(txtCoordinateY.Text),
                    coordinate_z = Convert.ToDouble(txtCoordinateZ.Text),
                    depth = Convert.ToDouble(txtDepth.Text),
                    gas_pressure_value = Convert.ToDouble(txtGasPressureValue.Text),
                    measure_date_time = dtpMeasureDateTime.Value,
                    tunnel = selectTunnelSimple1.selected_tunnel,
                    bid = IdGenerator.NewBindingId()
                };
                // 坐标X
                gasPressure.Save();
                DrawGasGushQuantityPt(gasPressure);
            }
            else
            {
                GasPressure.coordinate_x = Convert.ToDouble(txtCoordinateX.Text);
                GasPressure.coordinate_y = Convert.ToDouble(txtCoordinateY.Text);
                GasPressure.coordinate_z = Convert.ToDouble(txtCoordinateZ.Text);
                GasPressure.depth = Convert.ToDouble(txtDepth.Text);
                GasPressure.gas_pressure_value = Convert.ToDouble(txtGasPressureValue.Text);
                GasPressure.measure_date_time = dtpMeasureDateTime.Value;
                GasPressure.tunnel = selectTunnelSimple1.selected_tunnel;
                GasPressure.Save();
                DelGasGushQuantityPt(GasPressure.bid, GasPressure.coal_seam);
                DrawGasGushQuantityPt(GasPressure);
            }
        }

        /// <summary>
        ///     取  消
        /// </summary>
        /// <params name="sender"></params>
        /// <params name="e"></params>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            // 关闭窗口
            Close();
        }

        /// <summary>
        ///     20140801SDE中添加瓦斯压力点
        /// </summary>
        private void DrawGasGushQuantityPt(GasPressure gasGushQuantityEntity)
        {
            var dCoordinateX = Convert.ToDouble(txtCoordinateX.Text);
            var dCoordinateY = Convert.ToDouble(txtCoordinateY.Text);
            var dCoordinateZ = Convert.ToDouble(txtCoordinateZ.Text);
            IPoint pt = new PointClass();
            pt.X = dCoordinateX;
            pt.Y = dCoordinateY;
            pt.Z = dCoordinateZ;
            var pLayer = DataEditCommon.GetLayerByName(DataEditCommon.g_pMap, LayerNames.LAYER_ALIAS_MR_WSYLD);
            if (pLayer == null)
            {
                MessageBox.Show(@"未找到瓦斯压力点图层,无法绘制瓦斯压力点图元。");
                return;
            }
            var pFeatureLayer = (IFeatureLayer)pLayer;
            IGeometry geometry = pt;
            var list = new List<ziduan>
            {
                new ziduan("bid", gasGushQuantityEntity.bid),
                new ziduan("mc", gasGushQuantityEntity.coal_seam.ToString()),
                new ziduan("addtime", DateTime.Now.ToString(CultureInfo.InvariantCulture))
            };
            var wsyl = gasGushQuantityEntity.gas_pressure_value.ToString(CultureInfo.InvariantCulture);
            var cdbg = gasGushQuantityEntity.coordinate_z.ToString(CultureInfo.InvariantCulture);
            var ms = gasGushQuantityEntity.depth.ToString(CultureInfo.InvariantCulture);
            if (DataEditCommon.strLen(cdbg) < DataEditCommon.strLen(ms))
            {
                var count = DataEditCommon.strLen(ms) - DataEditCommon.strLen(cdbg);
                for (var i = 0; i < count; i++)
                {
                    cdbg = " " + cdbg; // // 测点标高
                }
            }
            else if (DataEditCommon.strLen(cdbg) > DataEditCommon.strLen(ms))
            {
                var count = DataEditCommon.strLen(cdbg) - DataEditCommon.strLen(ms);
                for (var i = 0; i < count; i++)
                {
                    ms += " ";
                }
            }

            list.Add(new ziduan("wsyl", wsyl));
            list.Add(new ziduan("cdbg", cdbg));
            list.Add(new ziduan("ms", ms));

            var pfeature = DataEditCommon.CreateNewFeature(pFeatureLayer, geometry, list);
            if (pfeature != null)
            {
                MyMapHelp.Jump(pt);
                DataEditCommon.g_pMyMapCtrl.ActiveView.PartialRefresh(
                    (esriViewDrawPhase)34, null, null);
            }
        }

        /// <summary>
        ///     删除瓦斯信息
        /// </summary>
        /// <params name="bid">绑定ID</params>
        /// <params name="mc">煤层</params>
        private void DelGasGushQuantityPt(string bid, string mc)
        {
            var pLayer = DataEditCommon.GetLayerByName(DataEditCommon.g_pMap, LayerNames.LAYER_ALIAS_MR_WSYLD);
            var pFeatureLayer = (IFeatureLayer)pLayer;
            DataEditCommon.DeleteFeatureByWhereClause(pFeatureLayer, "bid='" + bid + "' and mc='" + mc + "'");
        }

        #region 绘制瓦斯压力点图元

        public const string GasPressurePt = "瓦斯压力点";

        public IPoint GasPressurePoint { get; set; }

        //private void DrawGasPressurePt(string coalseamNO)
        //{
        //    var drawspecial = new DrawSpecialCommon();
        //    ////获得当前编辑图层
        //    //IFeatureLayer featureLayer = (IFeatureLayer)DataEditCommon.g_pLayer;

        //    var sLayerAliasName = coalseamNO + "号煤层-" + GasPressurePt;
        //    //string sLayerAliasName = GAS_PRESSURE_PT;
        //    var featureLayer = drawspecial.GetFeatureLayerByName(sLayerAliasName);

        //    if (featureLayer == null)
        //    {
        //        //如果对应图层不存在，要自动创建图层
        //        //IFeatureLayer existFeaLayer = drawspecial.GetFeatureLayerByName(GAS_PRESSURE_PT);
        //        var workspace = DataEditCommon.g_pCurrentWorkSpace;
        //        var layerName = "GAS_PRESSURE_PT" + "_NO" + coalseamNO;
        //        //若MapControl不存在该图层，但数据库中存在该图层，则先删除之，再重新生成
        //        var dataset = drawspecial.GetDatasetByName(workspace, layerName);
        //        if (dataset != null) dataset.Delete();
        //        //自动创建图层
        //        var map = DataEditCommon.g_pMap;
        //        //drawspecial.CreateFeatureLayerByExistLayer(map, existFeaLayer, workspace, layerName, sLayerAliasName);
        //        featureLayer = drawspecial.CreateFeatureLayer(map, workspace, layerName, sLayerAliasName);
        //        if (featureLayer == null)
        //        {
        //            MessageBox.Show("未成功创建" + sLayerAliasName + "图层,无法绘制瓦斯压力点图元，请联系管理员。");
        //            return;
        //        }
        //    }

        //    ///2.绘制瓦斯压力点   
        //    var dCoordinateX = Convert.ToDouble(txtCoordinateX.Text);
        //    var dCoordinateY = Convert.ToDouble(txtCoordinateY.Text);
        //    var dCoordinateZ = Convert.ToDouble(txtCoordinateZ.Text);
        //    IPoint pt = new PointClass();
        //    pt.X = dCoordinateX;
        //    pt.Y = dCoordinateY;
        //    pt.Z = dCoordinateZ;
        //    var pDrawWSYLD = new DrawWSYLD("P", txtGasPressureValue.Text,
        //        txtCoordinateZ.Text, txtDepth.Text);
        //    var feature = featureLayer.FeatureClass.CreateFeature();

        //    IGeometry geometry = pt;
        //    DrawCommon.HandleZMValue(feature, geometry); //几何图形Z值处理
        //    feature.Shape = pt;
        //    feature.Store();

        //    var strValue = feature.get_Value(feature.Fields.FindField("OBJECTID")).ToString();
        //    DataEditCommon.SpecialPointRenderer(featureLayer, "OBJECTID", strValue, pDrawWSYLD.m_Bitmap);

        //    ///3.显示瓦斯压力点图层
        //    if (featureLayer.Visible == false)
        //        featureLayer.Visible = true;

        //    DataEditCommon.g_pMyMapCtrl.ActiveView.Refresh();
        //}

        #endregion 绘制瓦斯压力点图元
    }
}