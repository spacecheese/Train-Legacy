#ifndef SPLINE_SAMPLER_INCLUDED
#define SPLINE_SAMPLER_INCLUDED

float4 mul_rotation(float4 a, float4 b){
	float4 rotation = 
	{
		a.x * b.w + a.w * b.x + a.z * b.z - a.y * b.z,
		a.y * b.w + a.w * b.y + a.x * b.z - a.z * b.x,
		a.z * b.w + a.w * b.z + a.y * b.x - a.x * b.y,
		a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z
	};
	return rotation;
}

float4 angle_axis(float3 axis, float angle){
	// https://stackoverflow.com/a/17654730/4262406
	float halfAngle = angle * .5f;
	float4 rotation;
	rotation.xyz = sin(halfAngle) * axis.xyz;
	rotation.w = cos(halfAngle);
	
	return normalize(rotation);
}

float4 from_to_rotation(float3 a, float3 b){
	float4 rotation;
	rotation.xyz = cross(a, b);
	rotation.w = sqrt(dot(a, a) * dot(b, b)) + dot(a, b);
	return normalize(rotation);
}

float3 rotate(float4 a, float3 b){
	return b + 2.0f * cross(a.xyz, cross(a.xyz, b) + a.w * b);
}

void rotate_float(float4 a, float3 b, out float3 result){
	result = rotate(a, b);
}

void sample_spline_float(float3 start, float startAngle, float3 startHandle, 
float3 end, float endAngle, float3 endHandle, float time, 
out float3 samplePosition, out float4 sampleRotation){
	// start and end nodes are in world position
	// handles are relative to their parent
	float3 world_startHandle = start + startHandle;
	float3 world_endHandle = end + endHandle;

	float3 a = lerp(start, world_startHandle, time);
	float3 b = lerp(world_startHandle, world_endHandle, time);
	float3 c = lerp(world_endHandle, end, time);
	
	float3 ab = lerp(a, b, time);
	float3 bc = lerp(b, c, time);
	
	samplePosition = lerp(ab, bc, time);
	float3 tangent = bc - ab;
	float angle = lerp(startAngle, endAngle, time);
	float3 forward = {0,0,1};
	
	sampleRotation = 
	mul_rotation(from_to_rotation(forward, tangent), angle_axis(tangent, angle));
}

#endif // SPLINE_SAMPLER_INCLUDED