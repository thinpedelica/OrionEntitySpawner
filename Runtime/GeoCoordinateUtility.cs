using System;

namespace GeoCoordinateUtility
{
    public static class GeoCoordinateConverter
    {
        /// <summary>
        /// 楕円体の長半径(ITRF座標系GRS80楕円体)
        /// </summary>
        const double _a = 6378137d;
        /// <summary>
        /// 楕円体の逆扁平率(ITRF座標系GRS80楕円体)
        /// </summary>
        const double _F = 298.257222101;
        /// <summary>
        /// 平面直角座標系のX軸上における縮尺係数
        /// </summary>
        const double _m0 = 0.9999;

        /// <summary>
        /// 平面直交座標から緯度経度への変換。国土地理院の計算式に準拠。
        /// https://vldb.gsi.go.jp/sokuchi/surveycalc/surveycalc/algorithm/xy2bl/xy2bl.htm
        /// </summary>
        /// <param name="x0">変換元のx座標(メートル単位)</param>
        /// <param name="y0">変換元のy座標(メートル単位)</param>
        /// <param name="latOrigin_deg">平面直交座標系原点の緯度(度単位10進)</param>
        /// <param name="lonOrigin_deg">平面直交座標系原点の経度(度単位10進)</param>
        /// <returns>GeoPoint型。X:変換後の緯度, Y:変換後の経度。ともに度単位10進</returns>
        public static GeoPoint Coordinate2LatLon (double x0, double y0, double latOrigin_deg, double lonOrigin_deg)
        {
            // φ0, λ0
            double phi0_rad = Degree2Radian (latOrigin_deg);
            double lambda0_rad = Degree2Radian (lonOrigin_deg);

            // n
            double n = 1 / (2 * _F - 1);

            // A0~A5
            var A = new double[6];
            A[0] = 1d + Math.Pow (n, 2) / 4 + Math.Pow (n, 4) / 64;
            A[1] = -3d / 2d * (n - Math.Pow (n, 3) / 8 - Math.Pow (n, 5) / 64);
            A[2] = 15d / 16d * (Math.Pow (n, 2) - Math.Pow (n, 4) / 4);
            A[3] = -35d / 48d * (Math.Pow (n, 3) - 5d / 16d * Math.Pow (n, 5));
            A[4] = 315d / 512d * Math.Pow (n, 4);
            A[5] = -693d / 1280d * Math.Pow (n, 5);

            // β1~β5
            var beta = new double[6];
            beta[1] = 1d / 2d * n - 2d / 3d * Math.Pow (n, 2) + 37d / 96d * Math.Pow (n, 3) - 1d / 360d * Math.Pow (n, 4) - 81d / 512d * Math.Pow (n, 5);
            beta[2] = 1d / 48d * Math.Pow (n, 2) + 1d / 15d * Math.Pow (n, 3) - 437d / 1440d * Math.Pow (n, 4) + 46d / 105d * Math.Pow (n, 5);
            beta[3] = 17d / 480d * Math.Pow (n, 3) - 37d / 840d * Math.Pow (n, 4) - 209d / 4480d * Math.Pow (n, 5);
            beta[4] = 4397d / 161280d * Math.Pow (n, 4) - 11d / 504d * Math.Pow (n, 5);
            beta[5] = 4583d / 161280d * Math.Pow (n, 5);

            // δ1~δ6
            var delta = new double[7];
            delta[1] = 2 * n - 2d / 3d * Math.Pow (n, 2) - 2 * Math.Pow (n, 3) + 116d / 45d * Math.Pow (n, 4) + 26d / 45d * Math.Pow (n, 5) - 2854d / 675d * Math.Pow (n, 6);
            delta[2] = 7d / 3d * Math.Pow (n, 2) - 8d / 5d * Math.Pow (n, 3) - 227d / 45d * Math.Pow (n, 4) + 2704d / 315d * Math.Pow (n, 5) + 2323d / 945d * Math.Pow (n, 6);
            delta[3] = 56d / 15d * Math.Pow (n, 3) - 136d / 35d * Math.Pow (n, 4) - 1262d / 105d * Math.Pow (n, 5) + 73814d / 2835d * Math.Pow (n, 6);
            delta[4] = 4279d / 630d * Math.Pow (n, 4) - 332d / 35d * Math.Pow (n, 5) - 399572d / 14175d * Math.Pow (n, 6);
            delta[5] = 4174d / 315d * Math.Pow (n, 5) - 144838d / 6237d * Math.Pow (n, 6);
            delta[6] = 601676d / 22275d * Math.Pow (n, 6);

            // S_φ0, A_
            double A_ = _m0 * _a / (1 + n) * A[0];
            double S_phi0 = 0;
            for (var j = 1; j <= 5; j++)
            {
                S_phi0 += A[j] * Math.Sin (2 * j * phi0_rad);
            }
            S_phi0 = _m0 * _a / (1 + n) * (A[0] * phi0_rad + S_phi0);

            // ξ, η
            double Xi = (x0 + S_phi0) / A_;
            double Eta = y0 / A_;

            // ξ', η'
            double XiD = 0;
            for (var j = 1; j <= 5; j++)
            {
                XiD += beta[j] * Math.Sin (2 * j * Xi) * Math.Cosh (2 * j * Eta);
            }
            XiD = Xi - XiD;
            double EtaD = 0;
            for (var j = 1; j <= 5; j++)
            {
                EtaD += beta[j] * Math.Cos (2 * j * Xi) * Math.Sinh (2 * j * Eta);
            }
            EtaD = Eta - EtaD;

            // χ
            double Chi = Math.Asin (Math.Sin (XiD) / Math.Cosh (EtaD));

            // φ, λ(求める緯度, 経度)
            double phi_rad = 0;
            for (var j = 1; j <= 6; j++)
            {
                phi_rad += delta[j] * Math.Sin (2 * j * Chi);
            }
            phi_rad += Chi;
            double lambda_rad = lambda0_rad + Math.Atan (Math.Sinh (EtaD) / Math.Cos (XiD));

            var latLon = new GeoPoint ()
            {
                // 変換後の緯度
                X = Radian2Degree (phi_rad),
                // 変換後の経度
                Y = Radian2Degree (lambda_rad)
            };
            return latLon;
        }

        /// <summary>
        /// 緯度経度から平面直交座標への変換。国土地理院の計算式に準拠。Xは南北、Yは東西を表すことに注意。
        /// https://vldb.gsi.go.jp/sokuchi/surveycalc/surveycalc/algorithm/bl2xy/bl2xy.htm
        /// </summary>
        /// <param name="lat0_deg">変換元の緯度(度単位10進)</param>
        /// <param name="lon0_deg">変換元の経度(度単位10進)</param>
        /// <param name="latOrigin_deg">平面直交座標系原点の緯度(度単位10進)</param>
        /// <param name="lonOrigin_deg">平面直交座標系原点の経度(度単位10進)</param>
        /// <returns> GeoPoint型。X:変換後のX座標, Y:変換後のY座標。ともにメートル単位。</returns>
        public static GeoPoint LatLon2Coordinate (double lat0_deg, double lon0_deg, double latOrigin_deg, double lonOrigin_deg)
        {
            // φ0, λ0
            double phi0_rad = Degree2Radian (latOrigin_deg);
            double lambda0_rad = Degree2Radian (lonOrigin_deg);

            // φ, λ
            double phi_rad = Degree2Radian (lat0_deg);
            double lambda_rad = Degree2Radian (lon0_deg);

            // n
            double n = 1 / (2 * _F - 1);

            //A0~A5
            double[] A = new double[6];
            A[0] = 1 + Math.Pow (n, 2) / 4 + Math.Pow (n, 4) / 64;
            A[1] = -3d / 2d * (n - Math.Pow (n, 3) / 8 - Math.Pow (n, 5) / 64);
            A[2] = 15d / 16d * (Math.Pow (n, 2) - Math.Pow (n, 4) / 4);
            A[3] = -35d / 48d * (Math.Pow (n, 3) - 5d / 16d * Math.Pow (n, 5));
            A[4] = 315d / 512d * Math.Pow (n, 4);
            A[5] = -693d / 1280d * Math.Pow (n, 5);

            //α1~α5
            double[] alpha = new double[6];
            alpha[1] = 1d / 2d * n - 2d / 3d * Math.Pow (n, 2) + 5d / 16d * Math.Pow (n, 3) + 41d / 180d * Math.Pow (n, 4) - 127d / 288d * Math.Pow (n, 5);
            alpha[2] = 13d / 48d * Math.Pow (n, 2) - 3d / 5d * Math.Pow (n, 3) + 557d / 1440d * Math.Pow (n, 4) + 281d / 630d * Math.Pow (n, 5);
            alpha[3] = 61d / 240d * Math.Pow (n, 3) - 103d / 140d * Math.Pow (n, 4) + 15061d / 26880d * Math.Pow (n, 5);
            alpha[4] = 49561d / 161280d * Math.Pow (n, 4) - 179d / 168d * Math.Pow (n, 5);
            alpha[5] = 34729d / 80640d * Math.Pow (n, 5);

            // S_φ0, A_
            double S_phi0 = 0;
            for (var j = 1; j <= 5; j++)
            {
                S_phi0 += A[j] * Math.Sin (2 * j * phi0_rad);
            }
            S_phi0 = _m0 * _a / (1 + n) * (A[0] * phi0_rad + S_phi0);
            double A_ = _m0 * _a / (1 + n) * A[0];

            // λc, λs
            double lambda_c = Math.Cos (lambda_rad - lambda0_rad);
            double lambda_s = Math.Sin (lambda_rad - lambda0_rad);

            // t, t_
            double t = Math.Sinh (Atanh (Math.Sin (phi_rad)) - 2 * Math.Sqrt (n) / (1 + n) * Atanh (2 * Math.Sqrt (n) / (1 + n) * Math.Sin (phi_rad)));
            double t_ = Math.Sqrt (1 + Math.Pow (t, 2));

            // ξ', η'
            double XiD = Math.Atan (t / lambda_c);
            double EtaD = Atanh (lambda_s / t_);

            // x, y(求めるX座標とY座標)
            double x = 0;
            double y = 0;
            for (var j = 1; j <= 5; j++)
            {
                x += alpha[j] * Math.Sin (2 * j * XiD) * Math.Cosh (2 * j * EtaD);
                y += alpha[j] * Math.Cos (2 * j * XiD) * Math.Sinh (2 * j * EtaD);
            }
            x = A_ * (XiD + x) - S_phi0;
            y = A_ * (EtaD + y);

            return new GeoPoint ()
            {
                // 変換後のX座標
                X = x,
                    // 変換後のY座標
                    Y = y,
            };
        }

        public static GeoPoint ZoneId2LatLon(int zoneId) {
            double lat = 0.0;
            double lon = 0.0;

            switch (zoneId) {
                case 1:
                    lat = 33.0;
                    lon = 129.5;
                    break;
                case 2:
                    lat = 33.0;
                    lon = 131.0;
                    break;
                case 3:
                    lat = 36.0;
                    lon = 132.17;
                    break;
                case 4:
                    lat = 33.0;
                    lon = 133.50;
                    break;
                case 5:
                    lat = 36.0;
                    lon = 134.33;
                    break;
                case 6:
                    lat = 36.0;
                    lon = 136.0;
                    break;
                case 7:
                    lat = 36.0;
                    lon = 131.17;
                    break;
                case 8:
                    lat = 36.0;
                    lon = 138.50;
                    break;
                case 9:
                    lat = 36.0;
                    lon = 139.83;
                    break;
                // 以下略
            }

            return new GeoPoint ()
            {
                X = lat,
                Y = lon,
            };
        }

        private static double Degree2Radian (double degree) => degree * Math.PI / 180;
        private static double Radian2Degree (double radian) => radian * 180 / Math.PI;
        private static double Atanh (double x) => Math.Log ((1 + x) / (1 - x)) / 2;
    }

    /// <summary>
    /// 変換メソッドの戻り値用構造体。
    /// 平面直交座標の場合、X、YはそのままXとYを表す。
    /// 緯度経度の場合、Xは緯度、Yは経度を表す。
    /// </summary>
    public struct GeoPoint
    {
        public double X;
        public double Y;

        public GeoPoint (double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}