#version 330 core
out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoords;

uniform vec3 lightPos;
uniform vec3 viewPos;
uniform vec3 lightColor;
uniform vec3 objectColor;
uniform sampler2D screenTexture;

void main()
{
    // 法線ベクトルの正規化
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);
    
    // 拡散反射の計算
    float diff = max(dot(norm, lightDir), 0.0);
    
    // トゥーンシェーディングのステップ
    float levels = 4.0; // トゥーンシェーディングのステップ数
    diff = floor(diff * levels) / levels;
    
    // 照明の適用
    vec3 result = (0.1 + diff * lightColor) * objectColor; // 0.1はアンビエント成分

    // エッジ検出
    vec3 viewDir = normalize(viewPos - FragPos);
    float edgeThreshold = 0.3; // エッジの閾値
    float edgeFactor = dot(norm, viewDir);
    
    // エッジを黒くする
    if(edgeFactor < edgeThreshold)
    {
        FragColor = vec4(0.0, 0.0, 0.0, 1.0); // 黒いエッジを描画
    }
    else
    {
        // レンズディストーションの適用
        vec2 uv = TexCoords * 2.0 - 1.0; // [-1, 1] の範囲に正規化
        float r = length(uv);
        float theta = atan(uv.y, uv.x);
        
        // 中心からの距離に基づく非線形補正
        float distortFactor = 1.0 - 0.5 * r * r; // 中心の近くで歪みを補正
        vec2 distortUV = vec2(distortFactor * cos(theta), distortFactor * sin(theta));
        
        distortUV = (distortUV + 1.0) / 2.0; // [0, 1] の範囲に再正規化
        
        // 最終的な色を取得
        vec4 toonColor = vec4(result, 1.0);
        vec4 distortColor = texture(screenTexture, distortUV);
        
        // トゥーンシェーディングの色とディストーションの色をブレンド
        FragColor = mix(toonColor, distortColor, 0.5);
    }
}