## ADDED Requirements

### Requirement: Video recording lifecycle
The system SHALL start frame capture at the beginning of the attempt and SHALL stop frame capture when the attempt ends.

#### Scenario: Recording duration
- **WHEN** an attempt starts and then finishes
- **THEN** the recorded video covers the entire attempt duration

### Requirement: Frame capture output
The system SHALL capture every rendered frame for maximum smoothness and save frames as PNG files into:
`artifacts/<name>-<dateTime>-<uuid>/frames/`.

#### Scenario: Frames folder population
- **WHEN** an attempt is running
- **THEN** PNG frames are created in `artifacts/<attempt>/frames/` with sequential filenames

### Requirement: Video file path
The system SHALL save the video as `video.mp4` inside the artifacts folder:
`artifacts/<name>-<dateTime>-<uuid>/video.mp4`.

#### Scenario: Video saved to artifacts folder
- **WHEN** an attempt completes
- **THEN** `video.mp4` exists in the artifacts folder and its path is referenced in `result.json`

### Requirement: Video encoding with ffmpeg
After frame capture ends, the system SHALL invoke `ffmpeg` (available on PATH) to encode `video.mp4` from the frames. The video framerate SHALL match the effective capture rate (frames captured divided by attempt duration) to avoid time distortion.

#### Scenario: ffmpeg encoding
- **WHEN** frame capture ends successfully
- **THEN** `ffmpeg` is executed to encode `video.mp4` using the captured frames

### Requirement: Frames cleanup
After `video.mp4` is successfully produced, the system SHALL delete the `frames/` folder to reduce disk usage.

#### Scenario: Frames cleanup after success
- **WHEN** `video.mp4` is successfully created
- **THEN** `artifacts/<attempt>/frames/` no longer exists

### Requirement: Video failure handling
If frame capture fails to start, `ffmpeg` is not found on PATH, or encoding fails, the system SHALL mark the attempt result as `status = "fail"` with `failureType = "video_error"` and set `reason` to indicate the video failure.

#### Scenario: Recording failure
- **WHEN** frame capture fails to start or `ffmpeg` encoding fails
- **THEN** the system writes `result.json` with `status = "fail"`, `failureType = "video_error"`, and a friendly reason describing the failure
