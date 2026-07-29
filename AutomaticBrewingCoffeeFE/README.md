This is a [Next.js](https://nextjs.org) project bootstrapped with [`create-next-app`](https://nextjs.org/docs/app/api-reference/cli/create-next-app).

## BE → FE change log & guides

- Xem ảnh hưởng khi BE thay đổi và cách xử lý: [docs/BE-change-impact.md](./docs/BE-change-impact.md)

## Functional test checklist

- Danh sách toàn bộ chức năng để regression test: [docs/FE-functional-test-checklist.md](./docs/FE-functional-test-checklist.md)

## Getting Started

First, run the development server:

```bash
npm run dev
# or
yarn dev
# or
pnpm dev
# or
bun dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

### Backend environment

Copy `.env.example` to `.env.local` and set `API_PROXY_TARGET` to the backend origin. The default configuration targets the deployed API at `https://alpa.io.vn`, whose endpoints are under `/api/v1`; its SignalR notification hub is `/hubs/notification`. Keeping the proxy enabled makes local development work without browser CORS issues.

You can start editing the page by modifying `app/page.tsx`. The page auto-updates as you edit the file.

This project uses [`next/font`](https://nextjs.org/docs/app/building-your-application/optimizing/fonts) to automatically optimize and load [Geist](https://vercel.com/font), a new font family for Vercel.

## Learn More

To learn more about Next.js, take a look at the following resources:

- [Next.js Documentation](https://nextjs.org/docs) - learn about Next.js features and API.
- [Learn Next.js](https://nextjs.org/learn) - an interactive Next.js tutorial.

You can check out [the Next.js GitHub repository](https://github.com/vercel/next.js) - your feedback and contributions are welcome!

## Deploy on Vercel

The easiest way to deploy your Next.js app is to use the [Vercel Platform](https://vercel.com/new?utm_medium=default-template&filter=next.js&utm_source=create-next-app&utm_campaign=create-next-app-readme) from the creators of Next.js.

Check out our [Next.js deployment documentation](https://nextjs.org/docs/app/building-your-application/deploying) for more details.
"# AutomaticBrewingCoffee_FE-develop-clone" 
"# AutomaticBrewingCoffee_FE-develop" 
"# AutomaticBrewingCoffee_FE-develop" 
