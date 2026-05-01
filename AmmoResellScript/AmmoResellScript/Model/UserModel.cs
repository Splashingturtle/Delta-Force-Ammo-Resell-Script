using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmmoResellScript.Model
{
    public class UserModel
    {
        #region 目标子弹坐标
        public int TargetAmmoX { get; set; } = 1652;
        public int TargetAmmoY { get; set; } = 306;
        #endregion

        #region 200发托条坐标
        public int TwoHundredAmmoX { get; set; } = 1039;
        public int TwoHundredAmmoY { get; set; } = 594;
        #endregion

        #region 购买按钮坐标
        public int BuyButtonX { get; set; } = 2022;
        public int BuyButtonY { get; set; } = 975;
        #endregion

        #region 开始游戏按钮坐标
        public int StartGameButtonX { get; set; } = 205;
        public int StartGameButtonY { get; set; } = 58;
        #endregion

        #region 交易行按钮坐标
        public int TradeRowButtonX { get; set; } = 700;
        public int TradeRowButtonY { get; set; } = 56;
        #endregion

        #region 账户坐标
        public int AccountX { get; set; } = 2217;
        public int AccountY { get; set; } = 67;
        #endregion

        #region 价格区域
        public int PriceRegionLeftX { get; set; } = 2065;
        public int PriceRegionLeftY { get; set; } = 352;
        public int PriceRegionRightX { get; set; } = 2209;
        public int PriceRegionRightY { get; set; } = 384;
        #endregion

        #region 账户剩余哈弗币区域
        public int RemainingHaverCoinLeftX { get; set; } = 0;
        public int RemainingHaverCoinLeftY { get; set; } = 0;
        public int RemainingHaverCoinRightX { get; set; } = 0;
        public int RemainingHaverCoinRightY { get; set; } = 0;
        #endregion

        #region 烽火地带
        public int FengHuoX { get; set; } = 0;
        public int FengHuoY { get; set; } = 0;
        #endregion

        #region 全面战场
        public int QuanMianZhanChangX { get; set; } = 0;
        public int QuanMianZhanChangY { get; set; } = 0;
        #endregion

        //购买次数
        public int PurchaseCount { get; set; } = 2;
        //点击延迟
        public int ClickDelay { get; set; } = 2;

    }
}
