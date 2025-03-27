# EmguCV Homomorphic Filter

A C# Windows Forms application implementing a homomorphic filter for image enhancement using EmguCV.

## Overview

Homomorphic filtering is a frequency domain image processing technique used to enhance image contrast by simultaneously normalizing brightness across the image while enhancing contrast. This makes it particularly useful for:

- Improving images with non-uniform illumination
- Enhancing details in both dark and bright regions
- Reducing the effect of illumination variations while preserving reflectance features

## Implementation Details

This implementation:

1. **Processes color images** by applying the homomorphic filter to each BGR channel separately
2. **Uses the Butterworth filter** in the frequency domain 
3. **Implements the complete homomorphic filtering process**:
   - Log-transform the image (to convert multiplicative noise to additive)
   - Perform DFT (Discrete Fourier Transform)
   - Apply frequency filter that attenuates low frequencies and enhances high frequencies
   - Perform inverse DFT
   - Exponential transform to revert the log transformation
   - Apply robust normalization with outlier clipping (5%-95% range)

## Key Parameters

The filter behavior is controlled by these parameters:

- **gammaL = 2.4**: Low frequency gain (controls illumination impact)
- **gammaH = 0.3**: High frequency gain (controls reflectance enhancement)
- **c = 0.2**: Controls the steepness of the filter's transition
- **d0 = 5.0**: Cutoff frequency

## Requirements

- .NET Framework 4.5+
- EmguCV (OpenCV wrapper for .NET)
- Visual Studio 2019+

## User Interface

The application provides a simple interface with:
- Original image display
- Filter result display
- Load Image button
- Apply Homomorphic Filter button

## How It Works

The homomorphic filtering process is based on the illumination-reflectance model of image formation. An image can be represented as:

```
I(x,y) = L(x,y) × R(x,y)
```

Where:
- I(x,y) is the image intensity
- L(x,y) is the illumination component (varies slowly)
- R(x,y) is the reflectance component (varies rapidly at edges)

The filtering steps:

1. Take the natural logarithm: ln(I) = ln(L) + ln(R)
2. Apply Fourier transform: F{ln(I)} = F{ln(L)} + F{ln(R)}
3. Filter in frequency domain with H(u,v): H(u,v)×F{ln(I)}
4. Apply inverse Fourier transform
5. Take exponential: exp(result)

This separates and processes illumination and reflectance components differently.

## Performance Notes

- FFT operations are optimized using optimal DFT sizes
- Unsafe code with direct memory access is used for efficiency
- Robust normalization prevents extreme values from affecting visual quality

## License

MIT

## References

- Gonzalez, R.C., & Woods, R.E. Digital Image Processing (4th Edition)