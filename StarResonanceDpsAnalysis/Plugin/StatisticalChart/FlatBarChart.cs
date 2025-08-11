using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace StarResonanceDpsAnalysis.Plugin.Charts
{
    /// <summary>
    /// ��ƽ������ͼ�ؼ�
    /// </summary>
    public class FlatBarChart : UserControl
    {
        #region �ֶκ�����

        private readonly List<BarChartData> _data = new();
        private bool _isDarkTheme = false;
        private string _titleText = "";
        private string _xAxisLabel = "";
        private string _yAxisLabel = "";
        private bool _showLegend = true;

        // �߾�����
        private const int PaddingLeft = 60;
        private const int PaddingRight = 20;
        private const int PaddingTop = 40;
        private const int PaddingBottom = 100;

        // �ִ�����ɫ
        private readonly Color[] _colors = {
            Color.FromArgb(74, 144, 226),   // ��
            Color.FromArgb(126, 211, 33),   // ��
            Color.FromArgb(245, 166, 35),   // ��
            Color.FromArgb(208, 2, 27),     // ��
            Color.FromArgb(144, 19, 254),   // ��
            Color.FromArgb(80, 227, 194),   // ��
            Color.FromArgb(184, 233, 134),  // ǳ��
            Color.FromArgb(75, 213, 238),   // ����
            Color.FromArgb(248, 231, 28),   // ��
            Color.FromArgb(189, 16, 224)    // Ʒ��
        };

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                _isDarkTheme = value;
                ApplyTheme();
                Invalidate();
            }
        }

        public string TitleText
        {
            get => _titleText;
            set
            {
                _titleText = value;
                Invalidate();
            }
        }

        public string XAxisLabel
        {
            get => _xAxisLabel;
            set
            {
                _xAxisLabel = value;
                Invalidate();
            }
        }

        public string YAxisLabel
        {
            get => _yAxisLabel;
            set
            {
                _yAxisLabel = value;
                Invalidate();
            }
        }

        public bool ShowLegend
        {
            get => _showLegend;
            set
            {
                _showLegend = value;
                Invalidate();
            }
        }

        #endregion

        #region ���캯��

        public FlatBarChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | 
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            
            ApplyTheme();
        }

        #endregion

        #region ���ݹ���

        public void SetData(List<(string Label, double Value)> data)
        {
            _data.Clear();
            
            for (int i = 0; i < data.Count; i++)
            {
                _data.Add(new BarChartData
                {
                    Label = data[i].Label,
                    Value = data[i].Value,
                    Color = _colors[i % _colors.Length]
                });
            }
            
            Invalidate();
        }

        public void ClearData()
        {
            _data.Clear();
            Invalidate();
        }

        #endregion

        #region ��������

        private void ApplyTheme()
        {
            if (_isDarkTheme)
            {
                BackColor = Color.FromArgb(31, 31, 31);
                ForeColor = Color.White;
            }
            else
            {
                BackColor = Color.White;
                ForeColor = Color.Black;
            }
        }

        #endregion

        #region ���Ʒ���

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // �������
            g.Clear(BackColor);

            if (_data.Count == 0)
            {
                DrawNoDataMessage(g);
                return;
            }

            // �������ֵ
            var maxValue = _data.Max(d => d.Value);
            if (maxValue <= 0) return;

            // �����ͼ����
            var chartRect = new Rectangle(PaddingLeft, PaddingTop, 
                                        Width - PaddingLeft - PaddingRight,
                                        Height - PaddingTop - PaddingBottom);

            // ��������
            DrawGrid(g, chartRect, maxValue);

            // ������
            DrawAxes(g, chartRect, maxValue);

            // ��������
            DrawBars(g, chartRect, maxValue);

            // ���Ʊ���
            DrawTitle(g);
        }

        private void DrawNoDataMessage(Graphics g)
        {
            var message = "��������";
            var font = new Font("Microsoft YaHei", 12, FontStyle.Regular);
            var brush = new SolidBrush(_isDarkTheme ? Color.Gray : Color.DarkGray);
            
            var size = g.MeasureString(message, font);
            var x = (Width - size.Width) / 2;
            var y = (Height - size.Height) / 2;
            
            g.DrawString(message, font, brush, x, y);
            
            font.Dispose();
            brush.Dispose();
        }

        private void DrawGrid(Graphics g, Rectangle chartRect, double maxValue)
        {
            var gridColor = _isDarkTheme ? Color.FromArgb(64, 64, 64) : Color.FromArgb(230, 230, 230);
            using var gridPen = new Pen(gridColor, 1);

            // ����ˮƽ������
            for (int i = 0; i <= 10; i++)
            {
                var y = chartRect.Y + (float)chartRect.Height * i / 10;
                g.DrawLine(gridPen, chartRect.X, y, chartRect.Right, y);
            }
        }

        private void DrawAxes(Graphics g, Rectangle chartRect, double maxValue)
        {
            var axisColor = _isDarkTheme ? Color.FromArgb(128, 128, 128) : Color.FromArgb(180, 180, 180);
            using var axisPen = new Pen(axisColor, 1);
            using var textBrush = new SolidBrush(ForeColor);
            using var font = new Font("Microsoft YaHei", 9);

            // ����X��
            g.DrawLine(axisPen, chartRect.X, chartRect.Bottom, chartRect.Right, chartRect.Bottom);
            
            // ����Y��
            g.DrawLine(axisPen, chartRect.X, chartRect.Y, chartRect.X, chartRect.Bottom);

            // X���ǩ���������
            var barWidth = (float)chartRect.Width / _data.Count;
            for (int i = 0; i < _data.Count; i++)
            {
                var x = chartRect.X + barWidth * (i + 0.5f);
                var text = _data[i].Label;
                
                var size = g.MeasureString(text, font);
                
                // ���浱ǰ�任
                var oldTransform = g.Transform.Clone();
                
                // ��ת45�Ȼ����ı�
                g.TranslateTransform(x, chartRect.Bottom + 10);
                g.RotateTransform(45);
                g.DrawString(text, font, textBrush, 0, 0);
                
                // �ָ��任
                g.Transform = oldTransform;
            }

            // Y���ǩ
            for (int i = 0; i <= 10; i++)
            {
                var y = chartRect.Bottom - (float)chartRect.Height * i / 10;
                var value = maxValue * i / 10;
                var text = Common.FormatWithEnglishUnits(value);
                
                var size = g.MeasureString(text, font);
                g.DrawString(text, font, textBrush, chartRect.X - size.Width - 5, y - size.Height / 2);
            }

            // ���ǩ
            if (!string.IsNullOrEmpty(_xAxisLabel))
            {
                var size = g.MeasureString(_xAxisLabel, font);
                var x = chartRect.X + (chartRect.Width - size.Width) / 2;
                var y = chartRect.Bottom + 70;
                g.DrawString(_xAxisLabel, font, textBrush, x, y);
            }

            if (!string.IsNullOrEmpty(_yAxisLabel))
            {
                var size = g.MeasureString(_yAxisLabel, font);
                using var matrix = new Matrix();
                matrix.RotateAt(-90, new PointF(15, chartRect.Y + (chartRect.Height + size.Width) / 2));
                g.Transform = matrix;
                g.DrawString(_yAxisLabel, font, textBrush, 15, chartRect.Y + (chartRect.Height + size.Width) / 2);
                g.ResetTransform();
            }
        }

        private void DrawBars(Graphics g, Rectangle chartRect, double maxValue)
        {
            var barWidth = (float)chartRect.Width / _data.Count * 0.8f; // ��һЩ���
            var barSpacing = (float)chartRect.Width / _data.Count * 0.1f;

            for (int i = 0; i < _data.Count; i++)
            {
                var data = _data[i];
                var barHeight = (float)(data.Value / maxValue * chartRect.Height);
                
                var x = chartRect.X + i * (barWidth + barSpacing * 2) + barSpacing;
                var y = chartRect.Bottom - barHeight;
                
                var barRect = new RectangleF(x, y, barWidth, barHeight);
                
                // �������� - ��ƽ����ƣ��ޱ߿�
                using var brush = new SolidBrush(data.Color);
                g.FillRectangle(brush, barRect);
                
                // ������ֵ��ǩ
                var valueText = Common.FormatWithEnglishUnits(data.Value);
                using var font = new Font("Microsoft YaHei", 8);
                using var textBrush = new SolidBrush(ForeColor);
                
                var textSize = g.MeasureString(valueText, font);
                var textX = x + (barWidth - textSize.Width) / 2;
                var textY = y - textSize.Height - 5;
                
                g.DrawString(valueText, font, textBrush, textX, textY);
            }
        }

        private void DrawTitle(Graphics g)
        {
            if (string.IsNullOrEmpty(_titleText)) return;

            using var font = new Font("Microsoft YaHei", 14, FontStyle.Bold);
            using var brush = new SolidBrush(ForeColor);
            
            var size = g.MeasureString(_titleText, font);
            var x = (Width - size.Width) / 2;
            var y = 10;
            
            g.DrawString(_titleText, font, brush, x, y);
        }

        #endregion
    }

    /// <summary>
    /// ����ͼ������
    /// </summary>
    public class BarChartData
    {
        public string Label { get; set; } = "";
        public double Value { get; set; }
        public Color Color { get; set; }
    }
}