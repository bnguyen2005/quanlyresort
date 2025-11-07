UI Style Guide — Resort Deluxe

1. Brand & Color Palette
- Primary: #C8A97E (Main), #B89968 (Dark), #F6EFE6 (Light)
- Neutrals: Ink #1A1A1A, Muted #6B7280, Line #E5E7EB, BG #F8F9FA, Surface #FFFFFF
- Feedback: Success #16A34A, Warning #D97706, Danger #DC2626, Info #0EA5E9
- Lưu trong CSS variables: wwwroot/css/tokens.css

2. Typography
- Heading: Poppins; Body: Inter
- Sizes: H1–H6 theo tokens (--fs-h1 …), Body 16px, Small 14px
- Line-height: tight 1.25, snug 1.35, normal 1.6

3. Components
- Buttons: Primary (bg Primary, text #FFF, radius 12px, shadow-sm; hover Primary Dark), Outline (border 2px Primary, hover bg Primary Light), Secondary (bg Surface, border Line)
- Inputs: height 48px, border 2px Line, focus border Primary + glow
- Cards: radius 16px, border Line, shadow-sm
- Navbar/Footer: nền tối, link hover Primary
- Modal/Toast: radius 12–16px, shadow-md
- Trạng thái: hover tăng ~8–12%; focus outline rõ (WCAG); disabled opacity .5

4. Layout & Grid
- Grid 12 cột (mobile-first); Container max-width: 540/720/960/1140/1440
- Spacing: 4, 8, 12, 16, 24, 32, 40, 48, 64
- Section padding: 48–80 desktop, 32–48 mobile

5. Page Templates
- Home: Hero + CTA, Highlights (rooms/offers), Reviews, Newsletter
- Rooms List: Filter bar (date, guests, type), grid cards 2–3 cột
- Room Detail: Gallery + details + sticky booking card, reviews
- Booking: Summary + guest form + payment (VietQR)
- Contact: Form + resort info + map lazy
- 404: minh họa + link quay lại

6. Technical Application
- Import globals.css (tự import tokens.css)
- Dùng utilities (text-ink, bg-surface, shadow-*) và class cơ bản trong globals.css
- Data-sync: fetch cache: 'no-store' + ?_=${Date.now()}; SW chỉ cache assets tĩnh; API AsNoTracking() cho list; invalidate sau CRUD

7. Accessibility
- Focus-visible rõ, label/for, aria-live=polite cho toast, tránh id trùng

8. Examples
- Buttons:
  <button class="btn btn-primary">Đặt phòng</button>
  <button class="btn btn-outline">Xem chi tiết</button>
- Card:
  <div class="card"><div class="card-body">Nội dung thẻ</div></div>

—
Điều chỉnh theo brand thực tế khi chốt nhận diện.

