using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace GaryPortalAPI.Data
{
    public static class Extensions
    {
        public static IQueryable<TEntity> IncludeIf<TEntity>([NotNull] this IQueryable<TEntity> source, Func<TEntity, bool> predicate, params Expression<Func<TEntity, object>>[] navigationPropertyPaths)
        where TEntity : class
        {
            if (predicate(source.First()))
            {
                if (navigationPropertyPaths != null && navigationPropertyPaths.Length > 0)
                {
                    foreach (var navigationPropertyPath in navigationPropertyPaths)
                    {
                        source = source.Include(navigationPropertyPath);
                    }
                }
            }
                return source;
        }

        public static IQueryable<T> If<T>(this IQueryable<T> source, bool condition, Func<IQueryable<T>, IQueryable<T>> transform)
        {
            return condition ? transform(source) : source;
        }

        public static IQueryable<T> If<T>(this IQueryable<T> source, Func<T, bool> predicate, Func<IQueryable<T>, IQueryable<T>> transform)
        {
            return predicate(source.First()) ? transform(source) : source;
        }

    }

    public static class MatrixExtensions
    {
        /// <summary>
        /// Returns the row with number 'row' of this matrix as a 1D-Array.
        /// </summary>
        public static T[] GetRow<T>(this T[,] matrix, int row)
        {
            var rowLength = matrix.GetLength(1);
            var rowVector = new T[rowLength];

            for (var i = 0; i < rowLength; i++)
                rowVector[i] = matrix[row, i];

            return rowVector;
        }



        /// <summary>
        /// Sets the row with number 'row' of this 2D-matrix to the parameter 'rowVector'.
        /// </summary>
        public static void SetRow<T>(this T[,] matrix, int row, T[] rowVector)
        {
            var rowLength = matrix.GetLength(1);

            for (var i = 0; i < rowLength; i++)
                matrix[row, i] = rowVector[i];
        }



        /// <summary>
        /// Returns the column with number 'col' of this matrix as a 1D-Array.
        /// </summary>
        public static T[] GetCol<T>(this T[,] matrix, int col)
        {
            var colLength = matrix.GetLength(0);
            var colVector = new T[colLength];

            for (var i = 0; i < colLength; i++)
                colVector[i] = matrix[i, col];

            return colVector;
        }



        /// <summary>
        /// Sets the column with number 'col' of this 2D-matrix to the parameter 'colVector'.
        /// </summary>
        public static void SetCol<T>(this T[,] matrix, int col, T[] colVector)
        {
            var colLength = matrix.GetLength(0);

            for (var i = 0; i < colLength; i++)
                matrix[i, col] = colVector[i];
        }

        public static List<List<T>> To2dList<T>(this T[,] array2d)
        {
            List<List<T>> list = Enumerable.Range(0, array2d.GetLength(0))
                .Select(row => Enumerable.Range(0, array2d.GetLength(1))
                .Select(col => array2d[row, col]).ToList()).ToList();
            return list;
        }

        public static string GetLeftDiagonalStringFromCoord<T>(this T[,] array2d, int row, int col)
        {
            int tempRow = row; int tempCol = col;
            string diagonal = array2d[row, col].ToString();

            while (tempRow >= 0 && tempRow < array2d.GetLength(0) && tempCol >= 0 && tempCol < array2d.GetLength(1))
            {
                tempRow -= 1;
                tempCol -= 1;
                if (tempRow >= 0 && tempRow < array2d.GetLength(0) && tempCol >= 0 && tempCol < array2d.GetLength(1))
                {
                    Console.WriteLine($"Looking at {tempRow} {tempCol}");
                    diagonal = diagonal.Insert(0, array2d[tempRow, tempCol]?.ToString() ?? " ");
                }
            }
            tempRow = row; tempCol = col;
            while (tempRow >= 0 && tempRow < array2d.GetLength(0) && tempCol >= 0 && tempCol < array2d.GetLength(1))
            {
                tempRow += 1;
                tempCol += 1;
                if (tempRow >= 0 && tempRow < array2d.GetLength(0) && tempCol >= 0 && tempCol < array2d.GetLength(1))
                {
                    Console.WriteLine($"Looking at {tempRow} {tempCol}");
                    diagonal += array2d[tempRow, tempCol]?.ToString() ?? " ";
                }
            }
            return diagonal;
        }

        public static string GetRightDiagonalStringFromCoord<T>(this T[,] array2d, int row, int col)
        {
            int tempRow = row; int tempCol = col;
            string diagonal = array2d[row, col].ToString();

            while (tempRow >= 0 && tempRow < array2d.GetLength(0) && tempCol >= 0 && tempCol < array2d.GetLength(1))
            {
                tempRow += 1;
                tempCol -= 1;
                if (tempRow >= 0 && tempRow < array2d.GetLength(0) && tempCol >= 0 && tempCol < array2d.GetLength(1))
                {
                    diagonal = diagonal.Insert(0, array2d[tempRow, tempCol]?.ToString() ?? " ");
                }
            }
            tempRow = row; tempCol = col;
            while (tempRow >= 0 && tempRow < array2d.GetLength(0) && tempCol >= 0 && tempCol < array2d.GetLength(1))
            {
                tempRow -= 1;
                tempCol += 1;
                if (tempRow >= 0 && tempRow < array2d.GetLength(0) && tempCol >= 0 && tempCol < array2d.GetLength(1))
                {
                    diagonal += array2d[tempRow, tempCol]?.ToString() ?? " ";
                }
            }
            return diagonal;
        }

        public static IList<IList<T>> GetSecondaryDiagonals<T>(this T[,] array2d)
        {
            int rows = array2d.GetLength(0);
            int columns = array2d.GetLength(1);

            var result = new List<IList<T>>();

            // number of secondary diagonals
            int d = rows + columns - 1;
            int r, c;

            // go through each diagonal
            for (int i = 0; i < d; i++)
            {
                // row to start
                if (i < columns)
                    r = 0;
                else
                    r = i - columns + 1;
                // column to start
                if (i < columns)
                    c = i;
                else
                    c = columns - 1;

                // items from diagonal
                var diagonalItems = new List<T>();
                do
                {
                    diagonalItems.Add(array2d[r, c]);
                    r++;
                    c--;
                }
                while (r < rows && c >= 0);
                result.Add(diagonalItems);
            }

            return result;
        }
    }
}


