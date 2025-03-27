using System;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Numerics;

namespace winform_Homom
{
    public partial class Form1 : Form
    {

        private Mat _originalImage = null;
        private PictureBox _originalPictureBox;
        private PictureBox _resultPictureBox;
        private Button _loadImageButton;
        private Button _homomorphicFilterButton;

        public Form1()
        {
            InitializeComponent();
        }

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.tif";
                openFileDialog.Title = "Select an Image";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _originalImage = CvInvoke.Imread(openFileDialog.FileName, ImreadModes.Color);
                        _originalPictureBox.Image = _originalImage.ToBitmap();
                        _homomorphicFilterButton.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void HomomorphicFilterButton_Click(object sender, EventArgs e)
        {
            if (_originalImage == null)
            {
                MessageBox.Show("Please load an image first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Split BGR channels
                VectorOfMat bgrChannels = new VectorOfMat();
                CvInvoke.Split(_originalImage, bgrChannels);

                // Apply homomorphic filtering to each channel
                Mat filteredB = ApplyHomomorphicFilter(bgrChannels[0].Clone());
                Mat filteredG = ApplyHomomorphicFilter(bgrChannels[1].Clone());
                Mat filteredR = ApplyHomomorphicFilter(bgrChannels[2].Clone());

                // Merge channels back
                VectorOfMat filteredChannels = new VectorOfMat();
                filteredChannels.Push(filteredB);
                filteredChannels.Push(filteredG);
                filteredChannels.Push(filteredR);

                Mat resultBgrImage = new Mat();
                CvInvoke.Merge(filteredChannels, resultBgrImage);

                // Display the result
                _resultPictureBox.Image = resultBgrImage.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filter: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Mat ApplyHomomorphicFilter(Mat channel)
        {
            try
            {
                // Convert to float and add small value to avoid log(0)
                Mat floatImage = new Mat();
                channel.ConvertTo(floatImage, DepthType.Cv32F);
                CvInvoke.Add(floatImage, new ScalarArray(0.1), floatImage);

                // Take natural log
                Mat logImage = new Mat();
                CvInvoke.Log(floatImage, logImage);

                // 최적 크기 계산 (2의 거듭제곱)
                int m = CvInvoke.GetOptimalDFTSize(logImage.Rows);
                int n = CvInvoke.GetOptimalDFTSize(logImage.Cols);

                // 패딩
                Mat padded = new Mat();
                CvInvoke.CopyMakeBorder(logImage, padded, 0, m - logImage.Rows, 0, n - logImage.Cols,
                                        BorderType.Constant, new MCvScalar(0));

                // DFT를 위한 복소수 행렬 준비
                Mat complexI = new Mat();
                VectorOfMat planesVector = new VectorOfMat();
                planesVector.Push(padded);
                planesVector.Push(new Mat(padded.Size, DepthType.Cv32F, 1));
                CvInvoke.Merge(planesVector, complexI);

                // DFT 수행
                CvInvoke.Dft(complexI, complexI);

                // 필터 생성 및 적용을 위해 채널 분리
                VectorOfMat splitPlanesVector = new VectorOfMat();
                CvInvoke.Split(complexI, splitPlanesVector);

                // 필터 생성 - 보다 마일드한 파라미터 사용
                double gammaL = 2.4;    // 더 높게 조정 (저주파 감쇠 완화)
                double gammaH = 0.3;    // 더 낮게 조정 (고주파 향상 제한)
                double c = 0.2;         // 기울기를 완만하게
                double d0 = 5.0;       // 컷오프 주파수 조정

                // 필터 적용
                Mat realPlane = splitPlanesVector[0];
                Mat imagPlane = splitPlanesVector[1];
                ApplyButterworthFilter(realPlane, imagPlane, padded.Size, gammaL, gammaH, c, d0);

                // 필터링된 복소수 행렬 재결합
                VectorOfMat filteredPlanesVector = new VectorOfMat();
                filteredPlanesVector.Push(realPlane);
                filteredPlanesVector.Push(imagPlane);
                Mat filteredComplex = new Mat();
                CvInvoke.Merge(filteredPlanesVector, filteredComplex);

                // 역 DFT 적용 (스케일링 포함)
                CvInvoke.Dft(filteredComplex, filteredComplex, DxtType.Inverse | DxtType.Scale, 0);

                // 역변환 결과에서 실수부만 추출
                VectorOfMat resultPlanesVector = new VectorOfMat();
                CvInvoke.Split(filteredComplex, resultPlanesVector);
                Mat realPart = resultPlanesVector[0];

                // 지수 변환 (로그의 역변환)
                Mat expMat = new Mat();
                CvInvoke.Exp(realPart, expMat);

                // 원본 크기로 자르기
                Mat result = new Mat(expMat, new Rectangle(0, 0, channel.Cols, channel.Rows));

                // 적절한 범위로 정규화
                Mat normalizedResult = new Mat();
                // 극단값 클리핑을 위한 추가 단계
                double minVal = 0, maxVal = 0;
                Point minLoc = new Point(), maxLoc = new Point();
                CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                // 5% ~ 95% 값 범위를 사용해 극단값 제거
                double lowerBound = minVal + (maxVal - minVal) * 0.05;
                double upperBound = minVal + (maxVal - minVal) * 0.95;

                // 클리핑 및 8비트로 변환
                result.ConvertTo(normalizedResult, DepthType.Cv8U, 255.0 / (upperBound - lowerBound),
                                 -lowerBound * 255.0 / (upperBound - lowerBound));

                return normalizedResult;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Filter error: {ex.Message}");
                return channel.Clone();
            }
        }

        private void ApplyButterworthFilter(Mat realPlane, Mat imagPlane, Size size, double gammaL, double gammaH, double c, double d0)
        {
            int centerX = size.Width / 2;
            int centerY = size.Height / 2;

            unsafe
            {
                float* rPtr = (float*)realPlane.DataPointer.ToPointer();
                float* iPtr = (float*)imagPlane.DataPointer.ToPointer();
                int step = (int)realPlane.Step / sizeof(float);

                for (int y = 0; y < size.Height; y++)
                {
                    for (int x = 0; x < size.Width; x++)
                    {
                        // 중심으로부터의 거리 계산
                        double dx = x - centerX;
                        double dy = y - centerY;
                        double distance = Math.Sqrt(dx * dx + dy * dy);

                        // 호모모픽 필터 함수
                        double filterValue = (gammaH - gammaL) * (1 - Math.Exp(-c * (distance * distance) / (d0 * d0))) + gammaL;

                        // 현재 픽셀 위치의 오프셋 계산
                        int idx = y * step + x;

                        // 필터 적용
                        rPtr[idx] = (float)(rPtr[idx] * filterValue);
                        iPtr[idx] = (float)(iPtr[idx] * filterValue);
                    }
                }
            }
        }

        private void ApplyFilterToPlane(Mat plane, double gammaL, double gammaH, double c, double d0)
        {
            int centerX = plane.Cols / 2;
            int centerY = plane.Rows / 2;

            unsafe
            {
                for (int y = 0; y < plane.Rows; y++)
                {
                    for (int x = 0; x < plane.Cols; x++)
                    {
                        // 중심으로부터의 거리 계산
                        double dx = x - centerX;
                        double dy = y - centerY;
                        double distance = Math.Sqrt(dx * dx + dy * dy);

                        // 호모모픽 필터 함수
                        double filterValue = (gammaH - gammaL) * (1 - Math.Exp(-c * (distance * distance) / (d0 * d0))) + gammaL;

                        // 현재 픽셀에 필터 값 적용
                        float* p = (float*)plane.DataPointer.ToPointer();
                        p[y * plane.Cols + x] *= (float)filterValue;
                    }
                }
            }
        }

        private Mat CreateHomomorphicFilter(Size size, double gammaL, double gammaH, double c, double d0)
        {
            Mat filter = new Mat(size, DepthType.Cv32F, 2); // Complex image (2 channels)

            int centerX = size.Width / 2;
            int centerY = size.Height / 2;

            // Create the filter - using SetRealAt for 2-channel matrix
            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    // Calculate distance from center (shifted coordinates)
                    int shiftedX = (x > centerX) ? x - size.Width : x;
                    int shiftedY = (y > centerY) ? y - size.Height : y;
                    double distance = Math.Sqrt(shiftedX * shiftedX + shiftedY * shiftedY);

                    // Homomorphic filter function
                    double filterValue = (gammaH - gammaL) * (1 - Math.Exp(-c * (distance * distance) / (d0 * d0))) + gammaL;

                    // Set real component
                    CvInvoke.cvSetReal2D(filter.Ptr, y, x, filterValue);
                    // Imaginary component is 0 (already initialized to 0)
                }
            }

            return filter;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}