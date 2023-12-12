#ifndef SSR_H
#define SSR_H

#define UV_COORD_SCALE 100000000.0
#define MAX_DISTANCE_THRESHOLD_LOW 70.0
#define MAX_DISTANCE_THRESHOLD_HIGH 1000.0
#define MAX_COLOR_COMPONENT 250.0
#define BINARY_SEARCH_ITERATIONS 10000000
#define BLEND_THRESHOLD 0.1
#define BLEND_FACTOR 10.0
#define SMOOTHNESS_THRESHOLD 0.8
#define DEFAULT_STEP_SIZE 1.0
#define DEFAULT_THICKNESS 1.0
#define HIGH_DISTANCE_STEP_SIZE 10.0
#define HIGH_DISTANCE_THICKNESS 10.0

//UV座標からシーンの深度を取得する関数
float SceneDepth(float2 UV)
{
	return LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV), _ZBufferParams);
}

//UV座標からシーンの色を取得する関数
TEXTURE2D_X(_BlitTexture);
float3 SceneColor(float2 uv)
{
	uint2 pixelCoords = uint2(uv * _ScreenSize.xy);
	return  LOAD_TEXTURE2D_X_LOD(_BlitTexture, pixelCoords, 0).xyz;
}


//深度とスクリーン座標から位置を再構築する関数
float3 ReconstructPosFromDepth(float depth, float2 screenPos)
{
	float3 viewVector = mul(unity_CameraInvProjection, float4(screenPos.xy * 2 - 1, 0, -1));
	float3 viewDirection = mul(unity_CameraToWorld, float4(viewVector, 0));
	float3 cameraDirection = (-1 * mul((float3x3)UNITY_MATRIX_M, transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V))[2].xyz));

	float ViewDotCam = dot(viewDirection, cameraDirection);
	float3 ViewDivCam = viewDirection / ViewDotCam;
	float3 ViewMulDepth = ViewDivCam * depth;
	float3 pos = ViewMulDepth + _WorldSpaceCameraPos;
	return pos;
}

//位置とスクリーン座標から深度を再構築する関数
float ReconstructDepthFromPos(float3 pos, float2 screenPos)
{
	float3 viewVector = mul(unity_CameraInvProjection, float4(screenPos.xy * 2 - 1, 0, -1));
	float3 viewDirection = mul(unity_CameraToWorld, float4(viewVector, 0));
	float3 cameraDirection = (-1 * mul((float3x3)UNITY_MATRIX_M, transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V))[2].xyz));

	float ViewDotCam = dot(viewDirection, cameraDirection);
	float3 ViewDivCam = viewDirection / ViewDotCam;

	float3 ViewMulDepth = pos - _WorldSpaceCameraPos;

	float depth = ViewMulDepth.z / ViewDivCam.z;

	return depth;
}

//3D座標をスクリーン座標に変換する関数
float2 WorldToScreenPos(float3 pos)
{
	pos = normalize(pos - _WorldSpaceCameraPos) * (UV_COORD_SCALE) + _WorldSpaceCameraPos;
	float3 toCam = mul(unity_WorldToCamera, pos);
	float camPosZ = toCam.z;
	float height = 2 * camPosZ / unity_CameraProjection._m11;
	float width = _ScreenParams.x / _ScreenParams.y * height;
	float2 uvCoords;
	uvCoords.x = (toCam.x + width / 2) / width;
	uvCoords.y = (toCam.y + height / 2) / height;
	return uvCoords;
}

//スクリーン座標から3D座標に変換する関数
float3 ScreenToWorldPos(float2 screenPos)
{
	float depth = SceneDepth(screenPos);
	return ReconstructPosFromDepth(depth, screenPos);
}

//スクリーンスペース レフレクションの計算を行う関数
void SSR_float(float3 viewDir, float3 cameraDir, float3 _normal, float2 screenPosition, float _StepSize, int _MaxSteps, int _BinarySearchSteps, float _Thickness, float _Smoothness, float _MinSmoothness, out float3 col, out float3 blitSource)
{
	float3 blendCol = 0;
	blitSource = SceneColor(screenPosition);

	//スムーズネスが一定値以下の場合はカラーをブレンドを行わない
	if (_Smoothness < _MinSmoothness)
	{
		col = blendCol;
		return;
	}

	float3 _position = ScreenToWorldPos(screenPosition);
	
	//カメラからの距離が一定値を超える場合、サンプリングのパラメータを調整
	float d = distance(_position, _WorldSpaceCameraPos);
	if (d > MAX_DISTANCE_THRESHOLD_LOW)
	{
		_StepSize = DEFAULT_STEP_SIZE;
		_Thickness = DEFAULT_THICKNESS;
	}
	if (d > MAX_DISTANCE_THRESHOLD_HIGH)
	{
		_StepSize = HIGH_DISTANCE_STEP_SIZE;
		_Thickness = HIGH_DISTANCE_THICKNESS;
	}

	//カメラからのレイの方向を計算
	float3 camRay = normalize(_position - _WorldSpaceCameraPos);
	float3 rayDir = normalize(reflect(camRay, normalize(_normal)));

	float distTravelled = 0;
	float prevDistance = 0;

	float3 color = 0;
	float2 uv = 0;
	float depth = _Thickness;

	UNITY_LOOP
	for (int j = 0; j < _MaxSteps; j++)
	{
		//最終ステップの場合に無限ループを回避するためにテキトーな大きな値で終了
		if (j == _MaxSteps - 1)
		{
			j = BINARY_SEARCH_ITERATIONS;
		}

		//前回のサンプリングからの距離を保存
		prevDistance = distTravelled;
		
		//サンプリング位置を更新
		distTravelled += _StepSize * (j + 1);

		//サンプリング位置からのレイの位置を計算
		float3 rayPos = _position + (rayDir * distTravelled);
		float3 projectedPos = ScreenToWorldPos(WorldToScreenPos(rayPos));

		//サンプリング位置と投影位置の距離を計算
		float projectedPosDist = distance(projectedPos, _WorldSpaceCameraPos);
		float rayPosDist = distance(rayPos, _WorldSpaceCameraPos);

		//SSRの深度を計算
		depth = rayPosDist - projectedPosDist;
		
		//SSRの深度が一定の範囲内に入った場合、正確な位置を見つける
		if (depth > 0 && depth < _Thickness * (j + 1) && j > 0)
		{
			UNITY_LOOP
			for (int k = 0; k < _BinarySearchSteps; k++)
			{
				//二分探索
				float midPointDist = (distTravelled + prevDistance) * 0.5;
				rayPos = _position + rayDir * midPointDist;
				//投影位置とサンプリング位置の距離で位置を補正
				if (distance(projectedPos, _WorldSpaceCameraPos) <= distance(rayPos, _WorldSpaceCameraPos))
				{
					distTravelled = midPointDist;
					uv = WorldToScreenPos(rayPos);
				}
				else 
				{
					prevDistance = midPointDist;
				}
			}
			break;
		}
	}
	//結果をカラーに格納
	col = min(SceneColor(uv), MAX_COLOR_COMPONENT * _Smoothness);

	//カラーブレンディング
	float3 c1 = SceneColor(uv);
	float3 c2 = SceneColor(screenPosition);
	if (c1.x == c2.x && c1.y == c2.y && c1.z == c2.z)
	{
		col = blendCol;
	}

	//画面端でのブレンディング処理
	if (uv.y < 0.1)
	{
		float blend = (clamp(uv.y, 0, 1)) * BLEND_FACTOR;
		col = lerp(col, blendCol, 1 - blend);
	}

	if (uv.y > 0.9)
	{
		float blend = (1 - clamp(uv.y, 0, 1)) * BLEND_FACTOR;
		col = lerp(col, blendCol, 1 - blend);
	}

	if (uv.x < 0.1)
	{
		float blend = (clamp(uv.x, 0, 1)) * BLEND_FACTOR;
		col = lerp(col, blendCol, 1 - blend);
	}

	if (uv.x > 0.9)
	{
		float blend = (1 - clamp(uv.x, 0, 1)) * BLEND_FACTOR;
		col = lerp(col, blendCol, 1 - blend);
	}

	 //ビューベクトルとカメラ方向のブレンディング
	float view = dot(rayDir, cameraDir);
	if (view < -0.1)
	{
		float blend = (-view - 0.5) * 2;
		blend = smoothstep(SMOOTHNESS_THRESHOLD, 1, 1 - blend);

		col = lerp(blendCol, col, blend);
		col = blendCol;
	}
}

#endif