using System;
using Microsoft.Xna.Framework;
using Rune.Monogame.Cameras;
using Rune.Monogame.Geometry;
using Rune.Monogame.Renderer;
using Plane = Rune.Monogame.Geometry.Plane;

namespace Rune.Monogame
{
    public class Grid
    {
        private class GridData
        {
            public int GridSize { get; set; }
            public Vector2i GridStart { get; set; }
            public Vector2i GridCount { get; set; }

            public Vector2 LineStart { get; set; }
            public Vector2 LineEnd { get; set; }
        }

        private readonly int _maxGridSize;
        private readonly Plane _upPlane = new Plane(Vector3.Up, 0);

        // set grid settings
        private Color _minorGridColor;
        private Color _majorGridColor;
        private Color _originGridColor;
        private int _gridSizeInUnits;
        private int _hideLinesLower;
        private int _majorLineEvery;

        public Grid(int maxGridSize)
        {
            // set grid settings
            _maxGridSize = maxGridSize;
            _minorGridColor = new Color(64, 64, 64);
            _majorGridColor = new Color(96, 96, 96);
            _originGridColor = new Color(160, 160, 160);
            _gridSizeInUnits = 1;
            _hideLinesLower = 2;
            _majorLineEvery = 8;
        }

        private Aabb CalculateViewportScreenBounds(Vector3 cameraPosition, float gridDim)
        {
            Aabb bounds = new Aabb();
            Plane upPlane = new Plane(Vector3.Up, 0);
            var boundsOffset = (Vector3.Right + Vector3.Forward) * gridDim;

            var distanceToPlane = upPlane.DistanceToPlane(cameraPosition);
            var position = cameraPosition - upPlane.Normal * distanceToPlane;

            bounds.Grow(position - boundsOffset);
            bounds.Grow(position + boundsOffset);

            return bounds;
        }

        private GridData CalculateGridData(Vector3 cameraPosition, out float gridDim)
        {
            GridData gridData = new GridData();
            var boundDimensions = 196.0f;
            var zoomFactor = 48.0f;

            var distanceToPlane = Math.Abs(_upPlane.DistanceToPlane(cameraPosition));
            var zoom = Math.Max(1, distanceToPlane) / zoomFactor;

            // hide lines if grid is smaller than specified number
            var gridSize = _gridSizeInUnits;
            while (gridSize / zoom < _hideLinesLower)
            {
                gridSize = gridSize << 2;
                if (gridSize >= _maxGridSize << 1)
                    gridSize = _maxGridSize;
            }

            //            gridDim = Math.Max(boundDimensions, zoom * boundDimensions);
            gridDim = (float)Math.Log(gridSize + 1, 2) * boundDimensions;
            Aabb bounds = CalculateViewportScreenBounds(cameraPosition, gridDim);

            float gridWidth = bounds.Max.X - bounds.Min.X;
            float gridHeight = bounds.Max.Z - bounds.Min.Z;

            int gridCountX = (int) gridWidth / gridSize + 4;
            int gridCountY = (int) gridHeight / gridSize + 4;
            int gridStartX = (int) bounds.Min.X / gridSize - 1;
            int gridStartY = (int) bounds.Min.Z / gridSize - 1;

            // Set line start and line end in world space coordinates
            float lineStartX = gridStartX * gridSize;
            float lineStartY = gridStartY * gridSize;
            float lineEndX = (gridStartX + (gridCountX - 1)) * gridSize;
            float lineEndY = (gridStartY + (gridCountY - 1)) * gridSize;

            // keep line start and line end inside the grid dimensions
            float finalLineStartX = (lineStartX < -gridDim) ? -gridDim : lineStartX;
            float finalLineStartY = (lineStartY < -gridDim) ? -gridDim : lineStartY;
            float finalLineEndX = (lineEndX > gridDim) ? gridDim : lineEndX;
            float finalLineEndY = (lineEndY > gridDim) ? gridDim : lineEndY;

            gridData.GridSize = gridSize;
            gridData.GridCount = new Vector2i(gridCountX, gridCountY);
            gridData.GridStart = new Vector2i(gridStartX, gridStartY);
            gridData.LineStart = new Vector2(finalLineStartX, finalLineStartY);
            gridData.LineEnd = new Vector2(finalLineEndX, finalLineEndY);

            return gridData;
        }

        public void Draw(PrimitiveBatch batch, Camera camera)
        {
            GridData gridData = CalculateGridData(camera.Position, out float gridDim);
            batch.Begin(camera.View, camera.Projection);

            // the grid lines are ordered as minor, major, origin
            for (int lineType = 0; lineType < 3; lineType++)
            {
                Color lineColor = _minorGridColor;
                if (lineType == 1)
                    lineColor = _majorGridColor;
                else if (lineType == 2)
                    lineColor = _originGridColor;

                Color finalLinecolor = lineColor;
                Vector2i gridStart = gridData.GridStart;
                Vector2i gridCount = gridData.GridCount;
                Vector2 lineStart = gridData.LineStart;
                Vector2 lineEnd = gridData.LineEnd;

                // draw horizontal lines
                RenderLines(gridDim, gridStart.Y, gridStart.Y + gridCount.Y,
                    gridData.GridSize, lineType, index =>
                    {
                        Vector3 from = new Vector3(lineStart.X, 0, (float) index * gridData.GridSize);
                        Vector3 to = new Vector3(lineEnd.X, 0, (float) index * gridData.GridSize);

                        batch.DrawLine(from, to, finalLinecolor);
                    });

                // draw vertical lines
                RenderLines(gridDim, gridStart.X, gridStart.X + gridCount.X,
                    gridData.GridSize, lineType, index =>
                    {
                        Vector3 from = new Vector3((float) index * gridData.GridSize, 0, lineStart.Y);
                        Vector3 to = new Vector3((float) index * gridData.GridSize, 0, lineEnd.Y);

                        batch.DrawLine(from, to, finalLinecolor);
                    });
            }

            batch.End();
        }

        private void RenderLines(float gridDim, int gridStart, int gridEnd, int gridSize, int lineType,
            Action<int> lineRender)
        {
            for (int i = gridStart; i < gridEnd; ++i)
            {
                // skip lines that are out of bound
                if (i * gridSize < -gridDim || i * gridSize > gridDim)
                    continue;

                // skip any line that don't match the line type we're adding
                if (lineType == 0 && (i == 0 || (i % _majorLineEvery) == 0))
                    continue;

                if (lineType == 1 && (i == 0 || (i % _majorLineEvery) != 0))
                    continue;

                if (lineType == 2 && i != 0)
                    continue;

                lineRender(i);
            }
        }

        public void IncreaseGridSize()
        {
            _gridSizeInUnits = _gridSizeInUnits << 1;
            if (_gridSizeInUnits == _maxGridSize << 1)
            {
                _gridSizeInUnits = _maxGridSize;
            }
        }

        public void DecreaseGridSize()
        {
            _gridSizeInUnits = _gridSizeInUnits >> 1;
            if (_gridSizeInUnits == 0)
            {
                _gridSizeInUnits = 1;
            }
        }
    }
}