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
        public async Task ShouldThrowCriticalDependencyExceptionOnDeleteIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Guid someStudentId = Guid.NewGuid();

            SqlException sqlException = GetSqlException();

            var failedStudentStorageException =
                new FailedStudentStorageException(sqlException);

            var expectedStudentDependencyException =
                new StudentDependencyException(failedStudentStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(someStudentId))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Student> deleteStudentTask =
                this.studentService.RemoveStudentByIdAsync(someStudentId);

            // then
            await Assert.ThrowsAsync<StudentDependencyException>(() =>
                deleteStudentTask.AsTask());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCritical(It.Is(SameExceptionAs(
                    expectedStudentDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(someStudentId),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnDeleteIfDatabaseUpdateErrorOccursAndLogItAsync()
        {
            // given
            Guid someStudentId = Guid.NewGuid();
            var databaseUpdateException = new DbUpdateException();

            var failedStudentStorageException =
                new FailedStudentStorageException(databaseUpdateException);

            var expectedStudentDependencyException =
                new StudentDependencyException(failedStudentStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(someStudentId))
                    .ThrowsAsync(databaseUpdateException);

            // when
            ValueTask<Student> deleteStudentTask =
                this.studentService.RemoveStudentByIdAsync(someStudentId);

            // then
            await Assert.ThrowsAsync<StudentDependencyException>(() =>
                deleteStudentTask.AsTask());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(someStudentId),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnDeleteWhenDbUpdateConcurrencyExceptionOccursAndLogItAsync()
        {
            // given
            Guid randomStudentId = Guid.NewGuid();
            Guid inputStudentId = randomStudentId;
            var databaseUpdateConcurrencyException = new DbUpdateConcurrencyException();

            var lockedStudentException = new LockedStudentException(databaseUpdateConcurrencyException);

            var expectedStudentDependencyException =
                new StudentDependencyException(lockedStudentException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(inputStudentId))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<Student> deleteStudentTask =
                this.studentService.RemoveStudentByIdAsync(inputStudentId);

            // then
            await Assert.ThrowsAsync<StudentDependencyException>(() =>
                deleteStudentTask.AsTask());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(expectedStudentDependencyException))),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(inputStudentId),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnDeleteWhenExceptionOccursAndLogItAsync()
        {
            // given
            Guid randomStudentId = Guid.NewGuid();
            Guid inputStudentId = randomStudentId;
            var exception = new Exception();

            var expectedStudentServiceException =
                new StudentServiceException(exception);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(inputStudentId))
                    .ThrowsAsync(exception);

            // when
            ValueTask<Student> deleteStudentTask =
                this.studentService.RemoveStudentByIdAsync(inputStudentId);

            // then
            await Assert.ThrowsAsync<StudentServiceException>(() =>
                deleteStudentTask.AsTask());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(expectedStudentServiceException))),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(inputStudentId),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
