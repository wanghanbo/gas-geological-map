using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using System.Reflection;
using GIS.Properties;
using GIS.Common;
using System.Collections;
using System.Threading;
using System.Collections.Generic;

namespace GIS
{
    /// <summary>
    /// 绘制直线
    /// </summary>
    [Guid("baf77864-3c2b-4dcf-9252-beadd9df6ee2")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GIS.BasicGraphic.AddStraightFeatureLine")]
    public sealed class AddStraightFeatureLine : BaseTool
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Register(regKey);
            ControlsCommands.Register(regKey);
        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Unregister(regKey);
            ControlsCommands.Unregister(regKey);
        }

        #endregion
        #endregion

        private IHookHelper m_hookHelper = null;
        private IMapControl3 m_pMapControl;
        private ILayer m_pCurrentLayer;
        private IMap m_pMap;
        private IDisplayFeedback m_pFeedback;
        private bool m_bInUse;
        private IPointCollection m_pPointCollection;
        private IGeometryCollection m_GeometryCollection = null;
        private IArray m_ElementArray = new ArrayClass();
        private bool m_IsFirstPoint;
        public AddStraightFeatureLine()
        {
            //公共属性定义
            base.m_category = "基础图元绘制"; 
            base.m_caption = "绘制直线";  
            base.m_message = "绘制直线";  
            base.m_toolTip = "绘制直线";  
            base.m_name = "AddStraightFeatureLine";   
            try
            {
                base.m_bitmap = Resources.EditingLineTool16;
                base.m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "Resources.AddStraightFeatureLine.cur"));
            
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Class Methods

        /// <summary>
        /// 创建工具
        /// </summary>
        /// <params name="hook">程序实例</params>
        public override void OnCreate(object hook)
        {
            try
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = hook;
                if (m_hookHelper.ActiveView == null)
                {
                    m_hookHelper = null;
                }
            }
            catch
            {
                m_hookHelper = null;
            }

            if (m_hookHelper == null)
                base.m_enabled = false;
            else
                base.m_enabled = true;
        }
        public override bool Enabled
        {
            get
            {
                IFeatureLayer featureLayer =  DataEditCommon.g_pLayer as IFeatureLayer;
                if (featureLayer == null)
                {
                    return false;
                }
                else
                {
                    if (featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public override bool Checked
        {
            get
            {
                return base.Checked;
            }
        }
        /// <summary>
        /// 点击事件
        /// </summary>
        public override void OnClick()
        {
            m_pMapControl = DataEditCommon.g_pMyMapCtrl;

            DataEditCommon.InitEditEnvironment();
            DataEditCommon.CheckEditState();
            ///获得编辑目标图层            
            m_pCurrentLayer = DataEditCommon.g_pLayer;
            IFeatureLayer featureLayer = m_pCurrentLayer as IFeatureLayer;
            if (featureLayer == null)
            {
                MessageBox.Show(@"请选择绘制图层。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DataEditCommon.g_pMyMapCtrl.CurrentTool = null;
                return;
            }
            else
            {
                if (featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline)
                {
                    MessageBox.Show(@"请选择线状图层。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DataEditCommon.g_pMyMapCtrl.CurrentTool = null;
                    return;
                }
            }

            m_pMap = m_hookHelper.FocusMap;
            m_IsFirstPoint = true;
        }
        public override void OnKeyDown(int keyCode, int Shift)
        {
            if (keyCode == (int)Keys.Escape)
            {
                m_IsFirstPoint = true;
                m_pFeedback = null;
                m_bInUse = false;
                m_GeometryCollection = null;
                IGraphicsContainer pGra = m_pMapControl.Map as IGraphicsContainer;
                for (int i = 0; i < m_ElementArray.Count; i++)
                {
                    pGra.DeleteElement(m_ElementArray.get_Element(i) as IElement);
                }
                m_pMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                m_ElementArray.RemoveAll();
            }
        }
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            IPoint pMovePt = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            pMovePt = GIS.GraphicEdit.SnapSetting.getSnapPoint(pMovePt);
            //左键点击绘制
            if (Button == 1)
                if (m_IsFirstPoint == true)
                {
                    NewFeatureMouseDown(pMovePt);
                    m_IsFirstPoint = false;
                    
                }
                else
                {
                    m_IsFirstPoint = true;
                    NewFeatureMouseDown(pMovePt);
                    NewFeatureEnd();
                }           
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            IPoint pMovePt = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            pMovePt = GIS.GraphicEdit.SnapSetting.getSnapPoint(pMovePt);
            NewFeatureMouseMove(pMovePt);
            DataEditCommon.g_pAxMapControl.Focus();
        }

        /// <summary>
        /// 新建对象，添加点
        /// </summary>
        /// <params name="x"></params>
        /// <params name="y"></params>
        public void NewFeatureMouseDown(IPoint pPoint)
        {
            INewPolygonFeedback pPolyFeed;
            INewLineFeedback pLineFeed;
            try
            {
                IFeatureLayer pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                if (pFeatureLayer.FeatureClass == null) return;
                IActiveView pActiveView = (IActiveView)m_pMap;
                /// 如果是新开始创建的对象，则相应的创建一个新的Feedback对象；
                /// 否则，向已存在的Feedback对象中加点
                if (!m_bInUse)
                {
                    m_pMap.ClearSelection();  //清除地图选中对象
                    m_bInUse = true;
                    m_pFeedback = new NewLineFeedbackClass();
                    pLineFeed = (INewLineFeedback)m_pFeedback;
                    pLineFeed.Start(pPoint);
                    if (m_pFeedback != null)
                        m_pFeedback.Display = pActiveView.ScreenDisplay;
                }
                else
                {
                    if (m_pFeedback is INewMultiPointFeedback)
                    {
                        object obj = Missing.Value;
                        m_pPointCollection.AddPoint(pPoint, ref obj, ref obj);
                    }
                    else if (m_pFeedback is INewLineFeedback)
                    {
                        pLineFeed = (INewLineFeedback)m_pFeedback;
                        pLineFeed.AddPoint(pPoint);
                    }
                    else if (m_pFeedback is INewPolygonFeedback)
                    {
                        pPolyFeed = (INewPolygonFeedback)m_pFeedback;
                        pPolyFeed.AddPoint(pPoint);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        /// <summary>
        /// 新建对象过程中鼠标移动方法,产生Track效果
        /// 在Map.MouseMove事件中调用本方法
        /// </summary>
        /// <params name="x">鼠标X坐标，屏幕坐标</params>
        /// <params name="y">鼠标Y坐标，屏幕坐标</params>
        public void NewFeatureMouseMove(IPoint pt)
        {
            if ((!m_bInUse) || (m_pFeedback == null)) return;

            m_pFeedback.MoveTo(pt);
        }

        /// <summary>
        /// 完成新建对象，取得绘制的对象，并添加到图层中
        /// 建议在Map.DblClick或Map.MouseDown(Button = 2)事件中调用本方法
        /// </summary>
        public void NewFeatureEnd()
        {
            IGeometry pGeom = null;
            IPointCollection pPointCollection;
            object obj = Type.Missing;
            try
            {
                if (m_pFeedback is INewMultiPointFeedback)
                {
                    INewMultiPointFeedback pMPFeed = (INewMultiPointFeedback)m_pFeedback;
                    pMPFeed.Stop();
                    pGeom = (IGeometry)m_pPointCollection;

                    if (m_GeometryCollection == null)
                    {
                        m_GeometryCollection = new PointClass() as IGeometryCollection;
                    }

                    m_GeometryCollection.AddGeometryCollection(pGeom as IGeometryCollection);
                }
                else if (m_pFeedback is INewLineFeedback)
                {
                    INewLineFeedback pLineFeed = (INewLineFeedback)m_pFeedback;

                    if (m_GeometryCollection == null)
                    {
                        m_GeometryCollection = new PolylineClass() as IGeometryCollection;
                    }

                    IPolyline pPolyLine = pLineFeed.Stop();

                    pPointCollection = (IPointCollection)pPolyLine;
                    if (pPointCollection.PointCount < 2)
                        MessageBox.Show("至少输入两个节点");
                    else
                        pGeom = (IGeometry)pPointCollection;

                    m_GeometryCollection.AddGeometryCollection(pGeom as IGeometryCollection);
                }
                else if (m_pFeedback is INewPolygonFeedback)
                {
                    INewPolygonFeedback pPolyFeed = (INewPolygonFeedback)m_pFeedback;

                    if (m_GeometryCollection == null)
                    {
                        m_GeometryCollection = new PolygonClass() as IGeometryCollection;
                    }

                    IPolygon pPolygon;
                    pPolygon = pPolyFeed.Stop();
                    if (pPolygon != null)
                    {
                        pPointCollection = (IPointCollection)pPolygon;
                        if (pPointCollection.PointCount < 3)
                            MessageBox.Show("至少输入三个节点");
                        else
                            pGeom = (IGeometry)pPointCollection;

                        m_GeometryCollection.AddGeometryCollection(pGeom as IGeometryCollection);
                    }
                }

                CreateFeature(m_GeometryCollection as IGeometry);
                m_pFeedback = null;
                m_bInUse = false;
                m_GeometryCollection = null;
            }
            catch (Exception e)
            {
                m_pFeedback = null;
                m_bInUse = false;
                m_GeometryCollection = null;
                Console.WriteLine(e.Message.ToString());
            }
        }

        /// <summary>
        /// 根据点创建要素
        /// </summary>
        /// <params name="pGeom"></params>
        private void CreateFeature(IGeometry pGeom)
        {
            try
            {
                if (pGeom == null) return;
                if (m_pCurrentLayer == null) return;

                IWorkspaceEdit pWorkspaceEdit = DataEditCommon.g_CurWorkspaceEdit;// GetWorkspaceEdit();
                IFeatureLayer pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;

                pWorkspaceEdit.StartEditOperation();
                IFeature pFeature = pFeatureClass.CreateFeature();

                // 处理Z/M值
                DrawCommon.HandleZMValue(pFeature, pGeom, 0);

                pFeature.Shape = pGeom;
                pFeature.Store();
                pWorkspaceEdit.StopEditOperation();
                
                IActiveView pActiveView = (IActiveView)m_pMap;
                m_GeometryCollection = null;

                IGraphicsContainer pGra = m_pMapControl.Map as IGraphicsContainer;
                for (int i = 0; i < m_ElementArray.Count; i++)
                {
                    pGra.DeleteElement(m_ElementArray.get_Element(i) as IElement);
                }
                m_ElementArray.RemoveAll();
                m_pMap.SelectFeature(m_pCurrentLayer, pFeature);
                m_pMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics | esriViewDrawPhase.esriViewGeoSelection, null, null);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }
        #endregion 
    }
}
