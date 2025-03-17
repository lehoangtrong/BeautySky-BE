-- Bật IDENTITY_INSERT để chèn giá trị CarePlanId theo ý muốn
SET IDENTITY_INSERT CarePlan ON;

-- Chèn dữ liệu vào bảng CarePlan
INSERT INTO CarePlan (CarePlanId, SkinTypeId, PlanName, Description) VALUES
(1, 1, N'Chăm sóc da thường', N'Lộ trình chăm sóc da giúp duy trì độ ẩm và làm sạch, giữ cho da khỏe mạnh và mịn màng.'),
(2, 2, N'Chăm sóc da khô', N'Lộ trình giúp cấp ẩm sâu và dưỡng da khô, phục hồi làn da mềm mịn và sáng khỏe.'),
(3, 3, N'Chăm sóc da dầu', N'Lộ trình giúp kiểm soát dầu và ngăn ngừa mụn.'),
(4, 4, N'Chăm sóc da hỗn hợp', N'Lộ trình giúp cân bằng độ ẩm, kiểm soát dầu ở vùng chữ T và dưỡng ẩm cho vùng má.'),
(5, 5, N'Chăm sóc da nhạy cảm', N'Lộ trình chăm sóc da nhạy cảm, giúp làm dịu da, giảm kích ứng và bảo vệ da khỏi tác hại từ bên ngoài.');

-- Tắt IDENTITY_INSERT sau khi chèn xong
SET IDENTITY_INSERT CarePlan OFF;

-- Chèn dữ liệu vào bảng CarePlanStep
INSERT INTO CarePlanStep (CarePlanId, StepOrder, StepName, StepDescription) VALUES
(1, 1, N'Làm sạch', N'Sử dụng sữa rửa mặt dịu nhẹ, không chứa xà phòng để loại bỏ bụi bẩn và dầu thừa trên da.'),
(1, 2, N'Tẩy tế bào chết', N'Sử dụng sản phẩm tẩy tế bào chết nhẹ nhàng 2 lần mỗi tuần để giúp da sáng mịn và sạch sâu.'),
(1, 3, N'Thoa toner', N'Dùng toner cấp ẩm và cân bằng pH cho da.'),
(1, 4, N'Dưỡng ẩm', N'Sử dụng kem dưỡng ẩm nhẹ để duy trì độ ẩm cho da mà không gây nhờn.'),
(1, 5, N'Chống nắng', N'Thoa kem chống nắng mỗi ngày để bảo vệ da khỏi tác hại của tia UV.'),

(2, 1, N'Làm sạch', N'Dùng sữa rửa mặt dạng kem giúp bổ sung độ ẩm cho da.'),
(2, 2, N'Tẩy tế bào chết', N'Tẩy da chết nhẹ nhàng 1-2 lần/tuần để giúp da hấp thụ dưỡng chất tốt hơn.'),
(2, 3, N'Thoa toner', N'Sử dụng toner dưỡng ẩm để làm dịu và cân bằng da.'),
(2, 4, N'Serum dưỡng ẩm', N'Dùng serum chứa Hyaluronic Acid để cấp nước cho da khô.'),
(2, 5, N'Dưỡng ẩm', N'Sử dụng kem dưỡng chứa thành phần khóa ẩm như dầu dưỡng hoặc ceramide.'),

(3, 1, N'Làm sạch', N'Dùng sữa rửa mặt có chứa salicylic acid để loại bỏ dầu thừa.'),
(3, 2, N'Tẩy tế bào chết', N'Dùng BHA hoặc AHA để giúp làm sạch lỗ chân lông.'),
(3, 3, N'Thoa toner', N'Sử dụng toner kiềm dầu để kiểm soát bã nhờn.'),
(3, 4, N'Dưỡng ẩm', N'Dùng gel dưỡng ẩm để không làm da quá bí bách.'),
(3, 5, N'Chống nắng', N'Thoa kem chống nắng có khả năng kiềm dầu để bảo vệ da.'),

(4, 1, N'Làm sạch', N'Sử dụng sữa rửa mặt nhẹ nhàng, phù hợp với da hỗn hợp.'),
(4, 2, N'Tẩy tế bào chết', N'Tẩy da chết 1-2 lần/tuần để giữ da sáng khỏe.'),
(4, 3, N'Thoa toner', N'Sử dụng toner cấp ẩm cho vùng khô, toner kiềm dầu cho vùng chữ T.'),
(4, 4, N'Dưỡng ẩm', N'Dùng kem dưỡng kết hợp cấp nước và kiểm soát dầu.'),
(4, 5, N'Chống nắng', N'Thoa kem chống nắng để bảo vệ da khỏi tác động từ môi trường.'),

(5, 1, N'Làm sạch', N'Sử dụng sữa rửa mặt dịu nhẹ, không chứa cồn hoặc hương liệu.'),
(5, 2, N'Tẩy tế bào chết', N'Dùng tẩy tế bào chết dạng enzyme để tránh kích ứng da.'),
(5, 3, N'Thoa toner', N'Dùng toner làm dịu, không chứa cồn hoặc hương liệu nhân tạo.'),
(5, 4, N'Dưỡng ẩm', N'Dùng kem dưỡng có thành phần làm dịu da như centella asiatica.'),
(5, 5, N'Chống nắng', N'Thoa kem chống nắng vật lý để bảo vệ da nhạy cảm.');

-- Kiểm tra lại dữ liệu đã chèn
SELECT * FROM CarePlan;
SELECT * FROM CarePlanStep;
