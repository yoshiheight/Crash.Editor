using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Crash.Core
{
    /// <summary>
    /// 数学ユーティリティー。
    /// </summary>
    public static class MathUtil
    {
        // 以下のいくつかのメソッドでインライン展開を希望している理由（高速化以外）
        // ・構造体を渡す際のコピー回避（且つ readonly struct の場合に防衛的コピーされないことを期待。実際どうなるかは未確認だが）
        // 
        // in パラメーター修飾子を指定してもよかったが、サイズの大きい構造体用というわけではないので却下とした

        /// <summary>
        /// 値が指定範囲内かどうかを判定する。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange<T>(T value, T min, T max) where T : IComparable<T>
        {
            Verifier.Verify<ArgumentException>(max.CompareTo(min) >= 0);

            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }

        /// <summary>
        /// 値を指定範囲内に制限する。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            Verifier.Verify<ArgumentException>(max.CompareTo(min) >= 0);

            return value.CompareTo(min) < 0 ? min
                : value.CompareTo(max) > 0 ? max
                : value;
        }

        /// <summary>
        /// 下限を指定する。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ClampMin<T>(T value, T min) where T : IComparable<T>
        {
            return Max(value, min);
        }

        /// <summary>
        /// 上限を指定する。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ClampMax<T>(T value, T max) where T : IComparable<T>
        {
            return Min(value, max);
        }

        /// <summary>
        /// 小さい方の値を取得する。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Min<T>(T value1, T value2) where T : IComparable<T>
        {
            return value1.CompareTo(value2) < 0 ? value1 : value2;
        }

        /// <summary>
        /// 大きい方の値を取得する。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Max<T>(T value1, T value2) where T : IComparable<T>
        {
            return value1.CompareTo(value2) > 0 ? value1 : value2;
        }

        /// <summary>
        /// 近い方の値を取得する。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Magnet(int value, int first, int second)
        {
            var firstSubtract = Math.Abs(value - first);
            var secondSubtract = Math.Abs(value - second);
            return (firstSubtract < secondSubtract) ? first : second;
        }

        /// <summary>
        /// 近い方の値を取得する。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Magnet(double value, double first, double second)
        {
            var firstSubtract = Math.Abs(value - first);
            var secondSubtract = Math.Abs(value - second);
            return (firstSubtract < secondSubtract) ? first : second;
        }

        /// <summary>
        /// 小数点以下、最近接偶数丸め。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundToInt32(double value)
        {
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// 小数点以下、四捨五入。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundAwayFromZeroToInt32(double value)
        {
            return Convert.ToInt32(Math.Round(value, MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// 小数点以下、切り捨て。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TruncateToInt32(double value)
        {
            return Convert.ToInt32(Math.Truncate(value));
        }

        /// <summary>
        /// 10進数の桁数を算出する。
        /// </summary>
        public static int CalcDigits(int value)
        {
            var digits = 1;
            while ((value /= 10) != 0)
            {
                digits++;
            }
            return digits;
        }

        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<int> RangeStartToEndInclusive(int start, int end)
        {
            for (var i = start; i <= end; i++)
            {
                yield return i;
            }
        }
    }
}
