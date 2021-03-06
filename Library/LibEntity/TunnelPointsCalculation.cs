﻿// ******************************************************************
// 概  述：巷道左右帮点计算
// 作  者：杨小颖
// 创建日期：2013/12/24
// 版本号：1.0
// ******************************************************************

using System.Collections.Generic;
using LibGeometry;

namespace LibEntity
{
    public class TunnelPointsCalculation
    {
        #region Functions

        /// <summary>
        ///     计算仅含两个导线点的巷道的左右邦点
        /// </summary>
        /// <params name="wirePts"></params>
        /// <params name="verticesLeftRet"></params>
        /// <params name="verticesRightRet"></params>
        /// <returns></returns>
        private bool calc_left_and_right_vertics_with2_traver_points(WirePoint[] wirePts,
            ref Vector3_DW[] verticesLeftRet, ref Vector3_DW[] verticesRightRet)
        {
            if (wirePts == null)
            {
                return false;
            }

            int nTraversePtCnt = wirePts.Length;
            if (nTraversePtCnt != 2)
            {
                return false;
            }

            //仅含两个导线点
            if (nTraversePtCnt == 2)
            {
                var ptPre = new Vector2_DW(wirePts[0].coordinate_x, wirePts[0].coordinate_y);
                var ptNext = new Vector2_DW(wirePts[1].coordinate_x, wirePts[1].coordinate_y);

                Vector2_DW vecForwardDir = (ptNext - ptPre).Normalize();
                /*根据法线方向判断巷道左右邦. 
                 * 若垂直向量vecPerpendicular与vecForwardDir的叉积所得向量与Y轴夹角小于90度(二者点积大于0),,则认为该垂直向量为左帮方向
                 * x1*x2 + y1*y2 = 0//垂直 
                 * x1*y2 - x2*y1 = 0//平行 
                 * 
                 *点积 a●b=|a|*|b|*cos(w)=x1*x2+y1*y2=0 
                 *叉积 aXb=|a|*|b|*sin(w)=(x1*z2-z1*y2)x^+(z1*x2-z1*y2)y^+(x1*y2-y1*x2)z^(右手螺旋定则)=0 -->二维的表示平行四边形的面积
                 *混合积 (aXb)●c = |aXb|*|c|*cos(aXb,c) -->平行六面体的体积
                 */
                var vecPerpendicularLeft = new Vector2_DW(vecForwardDir.Y, -vecForwardDir.X);
                Vector2_DW vecPerpendicularRight = -vecPerpendicularLeft;
                Vector3_DW vecNormal = Vector3_DW.Cross(new Vector3_DW(vecForwardDir.X, vecForwardDir.Y, 0),
                    new Vector3_DW(vecPerpendicularLeft.X, vecPerpendicularLeft.Y, 0));
                if (vecNormal.Z < 0)
                {
                    Vector2_DW vecSwap = vecPerpendicularLeft;
                    vecPerpendicularLeft = vecPerpendicularRight;
                    vecPerpendicularRight = vecSwap;
                }

                var leftVertices = new List<Vector3_DW>();
                var rightVertices = new List<Vector3_DW>();

                Vector2_DW ptLeftPre = ptPre + vecPerpendicularLeft * wirePts[0].left_distance;
                Vector2_DW ptLeftNext = ptNext + vecPerpendicularLeft * wirePts[1].left_distance;
                Vector2_DW ptRightPre = ptPre + vecPerpendicularRight * wirePts[0].right_distance;
                Vector2_DW ptRightNext = ptNext + vecPerpendicularRight * wirePts[1].right_distance;

                leftVertices.Add(new Vector3_DW(ptLeftPre.X, ptLeftPre.Y, wirePts[0].coordinate_z));
                leftVertices.Add(new Vector3_DW(ptLeftNext.X, ptLeftNext.Y, wirePts[1].coordinate_z));
                rightVertices.Add(new Vector3_DW(ptRightPre.X, ptRightPre.Y, wirePts[0].coordinate_z));
                rightVertices.Add(new Vector3_DW(ptRightNext.X, ptRightNext.Y, wirePts[1].coordinate_z));

                verticesLeftRet = leftVertices.ToArray();
                verticesRightRet = rightVertices.ToArray();
            }
            return true;
        }

        /// <summary>
        ///     根据导线点计算巷道左右帮的点
        ///     前后两个导线点坐标一样的情况未处理，传入的导线点数据需要保证不重复.
        /// </summary>
        /// <params name="wirePts">导线点实体</params>
        /// <params name="verticesLeftBtmRet">out，根据导线点计算出的巷道左帮所有点</params>
        /// <params name="verticesRightBtmRet">out，根据导线点计算出的巷道右帮所有点</params>
        /// <returns></returns>
        public bool calc_left_and_right_vertics(WirePoint[] wirePts, ref Vector3_DW[] verticesLeftBtmRet,
            ref Vector3_DW[] verticesRightBtmRet)
        {
            if (wirePts == null)
            {
                return false;
            }

            int nTraversePtCnt = wirePts.Length;
            if (nTraversePtCnt < 1)
            {
                return false;
            }

            #region 仅含两个导线点

            if (nTraversePtCnt == 2)
            {
                bool bRet = calc_left_and_right_vertics_with2_traver_points(wirePts, ref verticesLeftBtmRet,
                    ref verticesRightBtmRet);
                if (bRet == false)
                {
                    return false;
                }
            }
            #endregion
            #region 大于等于三个点

            else
            {
                var lstLeftBtmVertices = new List<Vector3_DW>();
                var lstRightBtmVertices = new List<Vector3_DW>();

                #region For loop

                for (int i = 0; i < nTraversePtCnt - 2; i++)
                {
                    var lwDatasPreTmp = new[]
                    {
                        wirePts[i],
                        wirePts[i + 1]
                    };

                    var lwDatasNextTmp = new[]
                    {
                        wirePts[i + 1],
                        wirePts[i + 2]
                    };

                    Vector3_DW[] verticesLeftPreTmp = null;
                    Vector3_DW[] verticesRightPreTmp = null;
                    if (false ==
                        calc_left_and_right_vertics_with2_traver_points(lwDatasPreTmp, ref verticesLeftPreTmp,
                            ref verticesRightPreTmp))
                    {
                        return false;
                    }
                    Vector3_DW[] verticesLeftNextTmp = null;
                    Vector3_DW[] verticesRightNextTmp = null;
                    if (false ==
                        calc_left_and_right_vertics_with2_traver_points(lwDatasNextTmp, ref verticesLeftNextTmp,
                            ref verticesRightNextTmp))
                    {
                        return false;
                    }
                    var vertexMid2D = new Vector2_DW();
                    var vertexLeftMid = new Vector3_DW();
                    var vertexRightMid = new Vector3_DW();
                    //左邦中间的点
                    LineIntersectType lit =
                        ToolsMath_DW.LineXLine(new Vector2_DW(verticesLeftPreTmp[0].X, verticesLeftPreTmp[0].Y),
                            new Vector2_DW(verticesLeftPreTmp[1].X, verticesLeftPreTmp[1].Y),
                            new Vector2_DW(verticesLeftNextTmp[0].X, verticesLeftNextTmp[0].Y),
                            new Vector2_DW(verticesLeftNextTmp[1].X, verticesLeftNextTmp[1].Y), ref vertexMid2D);
                    if (lit == LineIntersectType.None) //有重复点,可能是这种情况eg:p0(0, 0), p1(2, 0),p2(1, 0), p3(4, 0)
                    {
                        vertexLeftMid.X = verticesLeftPreTmp[1].X;
                        vertexLeftMid.Y = verticesLeftPreTmp[1].Y;
                        vertexLeftMid.Z = lwDatasPreTmp[1].coordinate_z;
                    }
                    else
                    {
                        vertexLeftMid.X = vertexMid2D.X;
                        vertexLeftMid.Y = vertexMid2D.Y;
                        vertexLeftMid.Z = lwDatasPreTmp[1].coordinate_z;
                    }
                    //右邦中间的点
                    lit = ToolsMath_DW.LineXLine(new Vector2_DW(verticesRightPreTmp[0].X, verticesRightPreTmp[0].Y),
                        new Vector2_DW(verticesRightPreTmp[1].X, verticesRightPreTmp[1].Y),
                        new Vector2_DW(verticesRightNextTmp[0].X, verticesRightNextTmp[0].Y),
                        new Vector2_DW(verticesRightNextTmp[1].X, verticesRightNextTmp[1].Y), ref vertexMid2D);
                    if (lit == LineIntersectType.None) //有重复点,可能是这种情况eg:p0(0, 0), p1(2, 0),p2(1, 0), p3(4, 0)
                    {
                        vertexRightMid.X = verticesRightPreTmp[1].X;
                        vertexRightMid.Y = verticesRightPreTmp[1].Y;
                        vertexRightMid.Z = lwDatasPreTmp[1].coordinate_z;
                    }
                    else
                    {
                        vertexRightMid.X = vertexMid2D.X;
                        vertexRightMid.Y = vertexMid2D.Y;
                        vertexRightMid.Z = lwDatasPreTmp[1].coordinate_z;
                    }
                    //保存计算出来的点
                    //第一个顶点
                    if (i == 0)
                    {
                        lstLeftBtmVertices.Add(verticesLeftPreTmp[0]);
                        lstRightBtmVertices.Add(verticesRightPreTmp[0]);
                    }
                    //中间的顶点
                    lstLeftBtmVertices.Add(vertexLeftMid);
                    lstRightBtmVertices.Add(vertexRightMid);
                    //最后一个顶点
                    if (i == nTraversePtCnt - 3)
                    {
                        lstLeftBtmVertices.Add(verticesLeftNextTmp[1]);
                        lstRightBtmVertices.Add(verticesRightNextTmp[1]);
                    }
                } //end for 

                #endregion

                verticesLeftBtmRet = lstLeftBtmVertices.ToArray();
                verticesRightBtmRet = lstRightBtmVertices.ToArray();
            }

            #endregion

            return true;
        }

        #endregion
    }
}