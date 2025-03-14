ALTER TABLE CarePlanProduct
ADD UserID INT;

SELECT name 
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('CarePlanProduct');

ALTER TABLE CarePlanProduct
DROP CONSTRAINT FK__CarePlanP__Produ__04E4BC85;-- thay cái này thành cái Key ông tìm dc liên kết của CarePlanProduct với Product


ALTER TABLE CarePlanProduct
ADD FOREIGN KEY (UserId) REFERENCES Users(UserId);


ALTER TABLE CarePlanProduct
ADD CONSTRAINT PK_CarePlanProduct PRIMARY KEY (UserId, CarePlanId, StepId);