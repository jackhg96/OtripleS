﻿// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using OtripleS.Web.Api.Models.Students;
using OtripleS.Web.Api.Models.Students.Exceptions;
using Xunit;

namespace OtripleS.Web.Api.Tests.Unit.Services.Foundations.Students
{
    public partial class StudentServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnModifyIfSqlErrorOccursAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTime = GetRandomDateTime();
            Student someStudent = CreateRandomStudent();
            someStudent.UpdatedDate = randomDateTime;
            SqlException sqlException = GetSqlException();

            var failedStudentStorageException =
                new FailedStudentStorageException(sqlException);

            var expectedStudentDependencyException =
                new StudentDependencyException(failedStudentStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(someStudent.Id))
                    .ThrowsAsync(sqlException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTime())
                    .Returns(randomDateTime);

            // when
            ValueTask<Student> modifyStudentTask =
                this.studentService.ModifyStudentAsync(someStudent);

            // then
            await Assert.ThrowsAsync<StudentDependencyException>(() =>
                modifyStudentTask.AsTask());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTime(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(someStudent.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCritical(It.Is(SameExceptionAs(expectedStudentDependencyException))),
                    Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnModifyIfDbUpdateExceptionOccursAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTime = GetRandomDateTime();
            Student someStudent = CreateRandomStudent();
            var databaseUpdateException = new DbUpdateException();

            var failedStudentStorageException = 
                new FailedStudentStorageException(databaseUpdateException);

            var expectedStudentDependencyException =
                new StudentDependencyException(failedStudentStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(someStudent.Id));

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTime())
                    .Throws(databaseUpdateException);

            // when
            ValueTask<Student> modifyStudentTask =
                this.studentService.ModifyStudentAsync(someStudent);

            // then
            await Assert.ThrowsAsync<StudentDependencyException>(() =>
                modifyStudentTask.AsTask());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTime(),
                    Times.Once);


            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(someStudent.Id),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnModifyIfDbUpdateConcurrencyExceptionOccursAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTime = GetRandomDateTime();
            Student randomStudent = CreateRandomStudent();
            Student someStudent = randomStudent;
            someStudent.UpdatedDate = randomDateTime;
            var databaseUpdateConcurrencyException = new DbUpdateConcurrencyException();
            var lockedStudentException = new LockedStudentException(databaseUpdateConcurrencyException);

            var expectedStudentDependencyException =
                new StudentDependencyException(lockedStudentException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(someStudent.Id))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTime())
                    .Returns(randomDateTime);

            // when
            ValueTask<Student> modifyStudentTask =
                this.studentService.ModifyStudentAsync(someStudent);

            // then
            await Assert.ThrowsAsync<StudentDependencyException>(() =>
                modifyStudentTask.AsTask());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTime(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(someStudent.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(expectedStudentDependencyException))),
                    Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnModifyIfServiceExceptionOccursAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTime = GetRandomDateTime();
            Student randomStudent = CreateRandomStudent();
            Student someStudent = randomStudent;
            someStudent.UpdatedDate = randomDateTime;
            var serviceException = new Exception();

            var expectedStudentServiceException =
                new StudentServiceException(serviceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(someStudent.Id))
                    .ThrowsAsync(serviceException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTime())
                    .Returns(randomDateTime);

            // when
            ValueTask<Student> modifyStudentTask =
                this.studentService.ModifyStudentAsync(someStudent);

            // then
            await Assert.ThrowsAsync<StudentServiceException>(() =>
                modifyStudentTask.AsTask());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTime(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(someStudent.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(expectedStudentServiceException))),
                    Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}
