import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Docker 環境でのホットリロード対応
  webpack: (config) => {
    config.watchOptions = {
      poll: 1000,
      aggregateTimeout: 300,
    };
    return config;
  },
};

export default nextConfig;
