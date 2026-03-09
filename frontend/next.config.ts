import type { NextConfig } from "next";

const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

const nextConfig: NextConfig = {
  // Docker 環境でのホットリロード対応
  webpack: (config) => {
    config.watchOptions = {
      poll: 1000,
      aggregateTimeout: 300,
    };
    return config;
  },

  // XSS 緩和のためのセキュリティヘッダー
  async headers() {
    return [
      {
        source: "/(.*)",
        headers: [
          {
            key: "Content-Security-Policy",
            value: [
              "default-src 'self'",
              // Next.js の開発サーバーが eval を使用するため開発時に必要
              "script-src 'self' 'unsafe-eval' 'unsafe-inline'",
              // MUI の Emotion が動的にスタイルを挿入するため必要
              "style-src 'self' 'unsafe-inline'",
              `connect-src 'self' ${apiUrl}`,
              "img-src 'self' data:",
              "font-src 'self'",
              "frame-ancestors 'none'",
            ].join("; "),
          },
          {
            key: "X-Content-Type-Options",
            value: "nosniff",
          },
          {
            key: "X-Frame-Options",
            value: "DENY",
          },
          {
            key: "Referrer-Policy",
            value: "strict-origin-when-cross-origin",
          },
        ],
      },
    ];
  },
};

export default nextConfig;
